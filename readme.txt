Configuration

Application OneAzureStorageFS


Storage_file
The location where the application save all data.

Prefixes
The port thats used to Microsoft Azure Blob Sdk use to connect.

Log
If you need seed the request of API and the Responses you can turn it on, and execute the OneAzureStorageFS.exe.

<appSettings>
    <add key="Storage_file" value="c:\temp\storage\"/>
    <add key="Prefixes" value="http://*:80/"/>
    <add key="Log" value="true"/>
  </appSettings>
  
  
Application OneAzureStorageFSService

This is a service responsible to start OneAzureStorageFS and ensures that OneAzureStorageFS is run always.

To install use the command InstallUtil or SC.
  
