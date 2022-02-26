using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace ConsoleBackup
{
    public class CLIArgumentAttribute: Attribute
    {
        private string description;
        public string Description{get => description;}

        private string[] aliases;
        public ImmutableArray<string> Aliases{ get => aliases.ToImmutableArray();}
        public CLIArgumentAttribute(string[] aliases, string description)
        {
            this.aliases = aliases;
            this.description = description;

        } 

        public override bool Equals(object obj) => obj.GetType() == typeof(string) ? aliases.Contains(obj.ToString()) : obj is CLIArgumentAttribute attribute &&
                   base.Equals(obj) &&
                   EqualityComparer<object>.Default.Equals(TypeId, attribute.TypeId) &&
                   EqualityComparer<string[]>.Default.Equals(aliases, attribute.aliases) &&
                   Aliases.Equals(attribute.Aliases);
        

        public override int GetHashCode() => base.GetHashCode();

        public static ImmutableDictionary<CLIArgumentAttribute, MethodInfo> GetAll(params System.Type[] baseTypes)
             => Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(i => i.IsClass && (baseTypes.Length == 0 || baseTypes.Contains(i)))
                        .Select(i => i.GetMethods()
                                      .Where(i => !(i.GetCustomAttribute<CLIArgumentAttribute>(false) is null))
                        ).Aggregate((cur, next) => cur.Concat(next)).ToDictionary(
                            k => k.GetCustomAttribute<CLIArgumentAttribute>(),
                            v => v
                        ).ToImmutableDictionary();
    }
}