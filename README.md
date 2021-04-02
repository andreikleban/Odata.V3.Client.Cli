# Odata.V3.Client.Cli

Odata.V3.Client.Cli is a command-line tool for generation OData client proxy classes. It's based on [OData Connected Services Extension](https://github.com/OData/ODataConnectedService).


# Command-line arguments


| Argument                  | Description |
| -------------             | ----------- |
| **-m**<br>**--metadata**  |The URI of the metadata document. The value must be set to a valid service document URI or a local file path.<br><br>Examples:<br> ```https://services.odata.org/V3/OData/OData.svc/```<br>```https://services.odata.org/V3/OData/OData.svc/$metadata```<br>```c:\temp\metadata.xml```<br>```c:\temp\metadata.edmx``` |
| **-o**<br>**--outputdir** |Full path to output directory. Current directory is using as a default.|
| **-f**<br>**--filename**  |Output file name. As a result of generation two files ```.cs``` and ```.edmx``` will be created.<br>Default filename is ```OdataService```|
|**-ns**<br>**--namespace** |Output classes namespace.<br>Namespaces from metadata document are using as a default.|
|**-v**<br>**--verbose**    |Turns on the console output.| 
|**-p**<br>**--proxy**      |Proxy server settings.<br>It needs for access to outside Odata V3 service from private networks.<br>Format: ```domain\user:password@SERVER:PORT```
|**-pl**<br>**--plugins**   |List of postprocessing plugins.<br>Format: ```Assembly.dll,Namespace.Class```|


# Examples


OData V3 metadata endpoint and relative output directory:
```
  Odata.V3.Client.Cli.exe -m https://services.odata.org/V3/OData/OData.svc/$metadata -o ClientDirectory
```
OData V3 service endpoint, relative output directory and filename:
```
  Odata.V3.Client.Cli.exe -m https://services.odata.org/V3/OData/OData.svc -o ClientDirectory -f OdataV3Client
```
Metadata document, absolute output directory, output classes namespace and verbocity:
```
  Odata.V3.Client.Cli.exe -m c:\temp\metadata.xml -o c:\temp\OutClientDir -ns Client.Namespace -v
```
OData V3 metadata endpoint and proxy server settings:
```
  Odata.V3.Client.Cli.exe -m https://services.odata.org/V3/OData/OData.svc/$metadata -p domain\user:userpassword@proxyserver:8080
```
Metadata document and list of postprocessing plugins:
```
  Odata.V3.Client.Cli.exe -m c:\temp\metadata.xml -pl Plugin1Assembly.dll,Namespace.PluginClass1 -pl Plugin2Assembly.dll,Namespace.PluginClass2
```
