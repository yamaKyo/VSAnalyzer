using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using System.Xml.Linq;


namespace VSAnalyzer
{
    class VCppConfig
    {
        private readonly XDocument doc;
        private readonly XElement root;
        private readonly IDictionary<string, XElement> configs;

        private VCppConfig(XDocument doc)
        {
            this.doc = doc;
            this.root = this.doc.Root;
            var configs = root.Descendants(root.GetDefaultNamespace() + "ItemDefinitionGroup");
            this.configs = new Dictionary<string, XElement>();
            
            foreach(var c in configs)
            {
                var x = c.Attribute("Condition").Value.Split("==")[1].Trim('\'').Trim();
                this.configs.Add(x, c);
            }
        }

        public static VCppConfig Parse(string name)
        {
            var doc = XDocument.Load(name);

            if(doc != null)
            {
                return new VCppConfig(doc);
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<string> get_config_names()
        {
            return this.configs.Keys;
        }

        public ISet<string> get_includes(string config_name)
        {
            return search_element(config_name, "AdditionalIncludeDirectories", ";");
        }

        public ISet<string> get_definitions(string config_name)
        {
            return search_element(config_name, "PreprocessorDefinitions", ";");
        }

        private ISet<string> search_element(string config_name, string element_name, string spliter = "")
        {
            if (this.configs.ContainsKey(config_name))
            {
                var element = this.configs[config_name];
                var n = element.GetDefaultNamespace();
                ISet<string> ret = new HashSet<string>();
                foreach (var e in element.Descendants(n + element_name))
                {
                    var val = e.Value;

                    if (spliter.Length > 0)
                    {
                        foreach (var i in val.Split(";"))
                        {
                            ret.Add(i);
                        }
                    }
                    else
                    {
                        ret.Add(val);
                    }
                }
                return ret;
            }
            else
            {
                return new HashSet<string>();
            }
        }
    }
}
