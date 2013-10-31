using System.Linq;
using System.Xml.Linq;

namespace OptimaJet.Common
{
    public static class Extensions
    {
        public static XElement SingleOrDefault(this XElement element, string name)
        {
            return element.Name == name ? element : element.Elements(name).SingleOrDefault();
        }
    }
}