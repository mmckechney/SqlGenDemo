# SQL Generation and Execution Demo

Demo app to show the use of Semantic Kernel and Azure OpenAI Agents as a means to generate SQL queries from a user query and then execute that query against a database to return a markdown table.

## Getting started

The easiest way to see this in action is to run the deployment script `deploy.ps1`. This script will create the necessary Azure resources and create a local.settings.json file for the SqlGenDemoConsole app. \
**NOTE**: since the app uses the gpt-4o with the assistants API, the `-aoaiLocation` must be a location that supports this. As of 11/7/2024, these regions are:

- australiaeast
- eastus
- eastus2
- francecentral
- norwayeast
- swedencentral
- uksouth
- westus3
- westus
- japaneast

``` Powershell
.\deploy.ps1 -appname "<uniquename>" -location "<azure region for most resource>" -aoaiLocation "<azure region for Azure OpenAI>"
```

The resources created are:

- *Azure OpenAI* - service instance with `gpt-4o` and `text-embedding-ada-002` model deployments
- *Azure SQL Server/Elastic Pool/Database*  - Database that will be used buy the database retrieval Semantic Kernel function
- *Azure AI Search* - used to index the table schema
-*Key Vault* - Used to store the secret keys for the above services

In addition, the script will:

- Grant the current user the Key Vault Secrets User role, so when running the console application, it can retrieve the secrets from Key Vault
- Create a `SqlGenDemoConsole/local.settings.json` file with the settings for the created resources.

## Prepping the Demo

1. Start the Console App

    - In a command window, navigate to the `SqlGenDemoConsole` folder.
    - Run `dotnet run` to run the app

2. Set up the Azure AI Search Index, database tables and sample data \
    (for your convenience, sample schema and data are provided in the `TableSchema` folder, but you can use any source that you'd like to fit your needs)

    a. Push the table schemas to the Azure AI Search Index

    - At the app command line, run the indexing

        ``` bash
            sqlgen> index --dir "<path to TableSchema folder>"
        ```

        You can select any directory that has the SQL schema that you would like to index. Please make sure that there is one table definition per file and the files have the `.sql` extension.

    b. Create the sample tables in the database

    - At the app command line, run the create command

        ``` bash
            sqlgen> index --dir "<path to TableSchema folder>"
        ```

        To ensure your index matches your database schema, the folder for this command should be the same as that for the `index` command

    c. Populate the database with sample data

    - At the app command line, run the create command

        ``` bash
            sqlgen> populate --dir "<path to data script folder>"
        ```

        This folder should have file(s) with INSERT scripts for your sample tables. Each file needs to have a `.data` ext

## Running the Demo

Now that the index and database have been set-up, go ahead and start asking your questions in the console app!

``` bash
sqlgen> what are the details on workorders?


-- Here are the details on work orders:

 Work_Order  Work_Date    Facility          Unit  Eq_Component_Tag  Equipment_Name  Equipment_Status  Equip_Status_Date
------------------------------------------------------------------------------------------------------------------------
 WO001       2023-01-10   Susquehanna       1     EQ1234            Pump            Active            2023-01-01
 WO002       2023-02-15   Three Mile Island 2     EQ5678            Valve           Inactive          2023-02-01
 WO003       2023-03-20   Peach Bottom      3     EQ9101            Generator       Active            2023-03-01
 WO004       2023-04-25   Limerick          4     EQ1121            Transformer     Inactive          2023-04-01        
 WO005       2023-05-30   Salem             5     EQ3141            Compressor      Active            2023-05-01
-----

```

NOTE: to remove the additional logging you will see, change the `UseFunctionInvocationFilter` value in the `local.settings.json` file to `false`
