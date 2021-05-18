// For the config file - Vigilance.dll
using Vigilance.API.Configs;

// Used for the Description attribute
using System.ComponentModel;

// Used for the List class
using System.Collections.Generic;

namespace Vigilance.External.Example
{
    public class ExampleConfig : IConfig
    {
        // Description of your config
        [Description("Determines whether the plugin is enabled or not")]

        // Make sure to always assign a default value to your configs
        // Make sure that your configs are serializable
        // Make sure that your configs always have a getter and a setter
        public bool IsEnabled { get; set; } = true;

        // An example list of items
        // The ItemType enum is defined in Assembly-CSharp.dll
        public List<ItemType> Items { get; set; } = new List<ItemType>
        {
            { ItemType.Medkit }
        };
    }
}
