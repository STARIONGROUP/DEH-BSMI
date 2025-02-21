# DEH-BSMI.Tools

Commandline application that retrives data from an ECSS-E-TM-10-25 data source and generates a requirements report that contains the requirements according to the structure in 10-25 and the BSMI.

## Usage:

The commandline application supports the following commandline options:

| option                  | alias  |  description |
| ----------------------- | ------ | ------------ | 
| --no-logo               |        | suppress showing the logo |
| --data-source           | -ds    | The URI of the ECSS-E-TM-10-25 data source |
| --username              | -u     | The username that is used to open the selected data source | 
| --password              | -p     | The password that is used to open the selected data source |
| --model                 | -m     | The EngineeringModel shortname |
| --iteration             | -i     | the Iteration number |
| --domainofexpertise     | -d     | The Domain of Expertise shortname |
| --source-specification  | --spec | The Specification from which the report is generated. If not specified all available non-deprecated specifications are taken into account |
| --unallocated-bsmi-code | -ubc   | the value of the BSMI parameter for unallocated requirements |
| --output-report         | -o     | path to report file |
| --auto-open-report      | -a     | Open the generated report with its default application |

The following gives a complete example that can be used on the publicly hosted server:

```
DEH-BSMI.Tools.exe excel-report --username admin --password pass --data-source http://cdp4services-public.cdp4.org --model LOFT --iteration 1 --domainofexpertise SYE --auto-open-report true
```

## Build and Release

To build and release the applicaton the following commands need to be run. These are combined in a batch file called `pack-n-release.bat`. Please note that the application is packaged as a self contained application. The resulting binary will be rather large since it contains the dotnet runtime, but makes it easier for the user since the dotnet runtine does not need to be installed. The application is published for three 64 bit target operation systems: Windows (10 and 11), Linux and OSX.

```
dotnet publish DEH-BSMI.Tools/DEH-BSMI.Tools.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ReleasePublication-Win64
dotnet publish DEH-BSMI.Tools/DEH-BSMI.Tools.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ReleasePublication-Linux64
dotnet publish DEH-BSMI.Tools/DEH-BSMI.Tools.csproj -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -o ReleasePublication-OSX64
```

## License and copyright

The DEH-BSMI.Tools commandline application is distributed with the APACHE 2.0 License.

## Process Flow

The application takes the following steps:

  - connects to the selected data source and opens the model/iteration
  - Generates a requirements sheet that contains a row per requirements
  - Generates per option a requirements sheet according to the BSMI structure:
    - Per option generate a nested element tree
    - iterates through the nested element tree
    - find the BSMI parameter per element and when found...
    - find the binary relationship where the source is an element definition and the target is a requirement
    - iterate though the requirements and find the one that is equal to the target in the just found binary relationship
    - store the combined `elementdefinition`-`requirement`-`BSMI` information in a list
    - iterate thriugh the `elementdefinition`-`requirement`-`BSMI` list and generate the excel spreadsheet


# Code Quality

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=STARIONGROUP_DEH-BSMI&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=STARIONGROUP_uml4net)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=STARIONGROUP_DEH-BSMI&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=STARIONGROUP_uml4net)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=STARIONGROUP_DEH-BSMI&metric=coverage)](https://sonarcloud.io/summary/new_code?id=STARIONGROUP_uml4net)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=STARIONGROUP_DEH-BSMI&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=STARIONGROUP_uml4net)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=STARIONGROUP_DEH-BSMI&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=STARIONGROUP_uml4net)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=STARIONGROUP_DEH-BSMI&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=STARIONGROUP_uml4net)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=STARIONGROUP_DEH-BSMI&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=STARIONGROUP_uml4net)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=STARIONGROUP_DEH-BSMI&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=STARIONGROUP_uml4net)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=STARIONGROUP_DEH-BSMI&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=STARIONGROUP_uml4net)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=STARIONGROUP_DEH-BSMI&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=STARIONGROUP_uml4net)

# Build Status

GitHub actions are used to build and test the uml4net libraries

Branch | Build Status
------- | :------------
Master | ![Build Status](https://github.com/STARIONGROUP/DEH-BSMI/actions/workflows/CodeQuality.yml/badge.svg?branch=master)
Development | ![Build Status](https://github.com/STARIONGROUP/DEH-BSMI/actions/workflows/CodeQuality.yml/badge.svg?branch=development)