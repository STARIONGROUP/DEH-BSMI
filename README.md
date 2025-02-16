# DEH-BSMI.Tools

Commandline application that retrives data from an ECSS-E-TM-10-25 data source and generates a requirements report that contains the requirements according to the structure in 10-25 and the BSMI.

## Usage:

The commandline application supports the following commandline options:

| option              | alias |  description |
| ------------------- | ----- | ------------ | 
| --no-logo           |       | suppress showing the logo |
| --data-source       | -ds   | The URI of the ECSS-E-TM-10-25 data source |
| --username          | -u    | The username that is used to open the selected data source | 
| --password          | -p    | The password that is used to open the selected data source |
| --model             | -m    | The EngineeringModel shortname |
| --iteration         | -i    | the Iteration number |
| --domainofexpertise | -d    | The Domain of Expertise shortname |
| --output-report     | -o    | path to report file |
| --auto-open-report  | -a    | Open the generated report with its default application |

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
