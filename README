# Voise Server

Main component of solution, Voise Server is the gateway among the client and the many cognitive providers, such as speech recognition (Speech-to-text), voice synthesizer (Text-to-speech) and intention recognition.

Currently follow providers are supported:

| Provedor | Tipo | STT | TTS | IR |
| -------- | :--: | :-: | :-: | :-: |
| Google Cloud | Saas | X | X | |
| Microsoft Azure | Saas | X | | |
| Microsoft Speech | On-premise | X | |
| Weka | On-premise | | | X |

## Requirements

* .Net Framework 4.8 Runtime (https://dotnet.microsoft.com/download/dotnet-framework/net48)
* If using the Microsoft Speech provider
    * Microsoft Speech Platform Runtime - x64
    * Language Packs SR (ou STT) and TTS of chosen languages

## Instalation

1. Compile the project
2. Move the artefacts to %ProgramFiles%/voise-server directory.
3. Create the Windows service, executing the follow command (as admin mode):

```
cd %ProgramFiles%\voise-server
Voise.Server.exe install
```
The service is created with automatic start.

## Configuration

The configuration is done in config.xml file in solution's main directory.

Example of configuration using the Google Cloud:
```
<?xml version="1.0" encoding="utf-8" ?>
<config>
    <!-- Default: 8102 -->
    <!--<port>8102</port>-->

    <recognizers>
        <!-- Default: me -->
        <enabled>ge</enabled>

        <google>
            <credential_path>.\credentials\google-credential.json</credential_path>
        </google>

    </recognizers>

    <synthesizers>
        <!-- Default: me -->
        <!--<enabled>me</enabled>-->
    </synthesizers>

    <!-- Default: ./classifiers/ -->
    <!-- <classifiers_path>./classifiers/</classifiers_path> -->

    <!-- WARNING: The log configuration must be directly done at Log.config file -->

    <tuning>
        <!-- Default: false -->
        <enabled>true</enabled>

        <!-- Default: ./tunning/ -->
        <!-- <path>./tunning/</path> -->

        <!-- Default: 7 -->
        <!-- <retention_days>7</retention_days> -->
    </tuning>
</config>
```
