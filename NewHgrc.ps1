@{
    NestedModules = @('INIFileParser.dll',
                        'NewHgrc.gll')

    PowerShellVersion = "3.0"

    FormatsToProcess = @()
    TypesToProcess = @()

    ModuleVersion = '1.0'

    AliasesToExport = @()
    FunctionsToExport = @('New-Hgrc')
}
