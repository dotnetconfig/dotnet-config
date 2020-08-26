namespace DotNetConfig
{
    enum ConfigAction
    {
        None,
        Add,
        Get,
        GetAll,
        GetRegexp,
        Set,
        SetAll, // aka Replace-All
        Unset,
        UnsetAll,

        List,
        Edit,

        // TODO:
        RenameSection,
        RemoveSection,
        /*
            --get                 get value: name [value-regex]
            --get-all             get all values: key [value-regex]
            --get-regexp          get values for regexp: name-regex [value-regex]
            --get-urlmatch        get value specific for the URL: section[.var] URL
            --replace-all         replace all matching variables: name value [value_regex]
            --add                 add a new variable: name value
            --unset               remove a variable: name [value-regex]
            --unset-all           remove all matches: name [value-regex]
            --rename-section      rename section: old-name new-name
            --remove-section      remove a section: name
            -l, --list            list all
            -e, --edit            open an editor
        */

    }
}
