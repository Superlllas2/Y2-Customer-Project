#!/usr/bin/env python3
"""Rebuild SDK-style IDE projects from Unity Bee rsp files (Cursor-safe)."""
from __future__ import annotations

import re
import uuid
from pathlib import Path
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[1]


def find_dag() -> Path:
    arts = ROOT / "Library/Bee/artifacts"
    dags = sorted(arts.glob("*.dag"), key=lambda p: p.stat().st_mtime, reverse=True)
    if not dags:
        raise SystemExit("No Library/Bee/artifacts/*.dag found. Open the project in Unity once.")
    return dags[0]


def parse_rsp(rsp_path: Path):
    defines, refs, sources, analyzers, langversion = [], [], [], [], "9.0"
    for raw in rsp_path.read_text(errors="replace").splitlines():
        line = raw.strip().strip('"')
        if not line:
            continue
        if line.startswith("/"):
            line = "-" + line[1:]
        if line.startswith("-define:"):
            defines.append(line[len("-define:") :])
        elif line.startswith("-r:"):
            refs.append(line[len("-r:") :].strip('"'))
        elif line.startswith("-analyzer:"):
            analyzers.append(line[len("-analyzer:") :].strip('"'))
        elif line.startswith("-langversion:"):
            langversion = line[len("-langversion:") :]
        elif line.startswith("-"):
            continue
        elif line.endswith(".cs"):
            sources.append(line.strip('"'))
    return defines, refs, sources, analyzers, langversion


def write_sdk_csproj(name: str, rsp_path: Path, extra_project_refs=None, extra_sources=None):
    defines, refs, sources, analyzers, langversion = parse_rsp(rsp_path)
    for s in extra_sources or []:
        if s not in sources:
            sources.append(s)

    script_asms = {p.stem: p for p in (ROOT / "Library/ScriptAssemblies").glob("*.dll")}
    rewritten_refs, seen = [], set()
    for r in refs:
        stem = Path(r).stem
        if stem == name or stem in seen:
            continue
        seen.add(stem)
        if stem in script_asms and "Unity.app/Contents" not in r and "PlaybackEngines" not in r:
            rewritten_refs.append(str(script_asms[stem].relative_to(ROOT)).replace("\\", "/"))
        else:
            rewritten_refs.append(r)

    lines = []
    a = lines.append
    a("<Project>")
    a("  <!-- Generated from Unity Bee rsp (SDK-style). -->")
    a("  <PropertyGroup>")
    a("    <BaseIntermediateOutputPath>Temp/obj/$(MSBuildProjectName)</BaseIntermediateOutputPath>")
    a("    <IntermediateOutputPath>$(BaseIntermediateOutputPath)</IntermediateOutputPath>")
    a("    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>")
    a("    <UseCommonOutputDirectory>true</UseCommonOutputDirectory>")
    a("    <OutputPath>Temp/bin/Debug/</OutputPath>")
    a("  </PropertyGroup>")
    a('  <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk" />')
    a("  <ItemGroup>")
    a('    <ProjectCapability Include="Unity" />')
    a('    <ProjectCapability Include="EnableOnDemandExcludedFolderLoading" />')
    a("  </ItemGroup>")
    a("  <PropertyGroup>")
    a("    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>")
    a("    <EnableDefaultItems>false</EnableDefaultItems>")
    a(f"    <LangVersion>{escape(langversion)}</LangVersion>")
    a("    <RootNamespace></RootNamespace>")
    a("    <OutputType>Library</OutputType>")
    a(f"    <AssemblyName>{escape(name)}</AssemblyName>")
    a("    <TargetFramework>netstandard2.1</TargetFramework>")
    a("    <BaseDirectory>.</BaseDirectory>")
    a("    <DefineConstants>" + escape(";".join(defines)) + "</DefineConstants>")
    a("  </PropertyGroup>")
    a("  <PropertyGroup>")
    a("    <NoStandardLibraries>true</NoStandardLibraries>")
    a("    <NoStdLib>true</NoStdLib>")
    a("    <NoConfig>true</NoConfig>")
    a("    <DisableImplicitFrameworkReferences>true</DisableImplicitFrameworkReferences>")
    a("    <MSBuildWarningsAsMessages>MSB3277</MSBuildWarningsAsMessages>")
    a("  </PropertyGroup>")
    a("  <ItemGroup>")
    for s in sources:
        a(f'    <Compile Include="{escape(s.replace(chr(92), "/"))}" />')
    a("  </ItemGroup>")
    a("  <ItemGroup>")
    for r in rewritten_refs:
        hint = r.replace("\\", "/")
        a(f'    <Reference Include="{escape(Path(hint).stem)}">')
        a(f"      <HintPath>{escape(hint)}</HintPath>")
        a("      <Private>False</Private>")
        a("    </Reference>")
    a("  </ItemGroup>")
    if analyzers:
        a("  <ItemGroup>")
        for an in analyzers:
            if Path(an).exists():
                a(f'    <Analyzer Include="{escape(an)}" />')
        a("  </ItemGroup>")
    if extra_project_refs:
        a("  <ItemGroup>")
        for pref in extra_project_refs:
            a(f'    <ProjectReference Include="{escape(pref)}" />')
        a("  </ItemGroup>")
    a('  <Import Project="Sdk.targets" Sdk="Microsoft.NET.Sdk" />')
    a("  <ItemGroup>")
    for cap in [
        "LaunchProfiles",
        "SharedProjectReferences",
        "ReferenceManagerSharedProjects",
        "ReferenceManagerProjects",
        "COMReferences",
        "ReferenceManagerCOM",
        "AssemblyReferences",
        "ReferenceManagerAssemblies",
    ]:
        a(f'    <ProjectCapability Remove="{cap}" />')
    a("  </ItemGroup>")
    a("</Project>")
    (ROOT / f"{name}.csproj").write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"Wrote {name}.csproj ({len(sources)} sources)")


def guid_for(name: str) -> str:
    return str(uuid.uuid5(uuid.NAMESPACE_URL, "unity:" + name)).upper()


def main():
    dag = find_dag()
    # Include scripts Unity has not recompiled yet
    extras = []
    for p in (ROOT / "Assets").rglob("*.cs"):
        rel = str(p.relative_to(ROOT)).replace("\\", "/")
        if "/Editor/" in rel or rel.endswith("/Editor.cs"):
            continue
        extras.append(rel)

    write_sdk_csproj("Assembly-CSharp", dag / "Assembly-CSharp.rsp", extra_sources=extras)
    write_sdk_csproj(
        "Assembly-CSharp-Editor",
        dag / "Assembly-CSharp-Editor.rsp",
        extra_project_refs=["Assembly-CSharp.csproj"],
        extra_sources=[
            str(p.relative_to(ROOT)).replace("\\", "/")
            for p in (ROOT / "Assets").rglob("*.cs")
            if "/Editor/" in str(p).replace("\\", "/")
        ],
    )

    g1, g2 = guid_for("Assembly-CSharp"), guid_for("Assembly-CSharp-Editor")
    (ROOT / "Y2-Customer-Project.sln").write_text(
        f"""Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 16
Project("{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}") = "Assembly-CSharp", "Assembly-CSharp.csproj", "{{{g1}}}"
EndProject
Project("{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}") = "Assembly-CSharp-Editor", "Assembly-CSharp-Editor.csproj", "{{{g2}}}"
EndProject
Global
\tGlobalSection(SolutionConfigurationPlatforms) = preSolution
\t\tDebug|Any CPU = Debug|Any CPU
\tEndGlobalSection
\tGlobalSection(ProjectConfigurationPlatforms) = postSolution
\t\t{{{g1}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
\t\t{{{g1}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
\t\t{{{g2}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
\t\t{{{g2}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
\tEndGlobalSection
EndGlobal
"""
    )
    (ROOT / "Y2-Customer-Project.slnx").write_text(
        """<Solution>
  <Project Path="Assembly-CSharp.csproj" />
  <Project Path="Assembly-CSharp-Editor.csproj" />
</Solution>
"""
    )
    print("Wrote Y2-Customer-Project.sln")


if __name__ == "__main__":
    main()
