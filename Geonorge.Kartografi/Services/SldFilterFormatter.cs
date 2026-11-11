using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Geonorge.Kartografi.Services
{
    /// <summary>Formats an OGC/FES filter as readable text, e.g. "avKlasse = 'Matfisk' AND areal > 10"</summary>
    public static class SldFilterFormatter
    {
        private static readonly Dictionary<string, string> ComparisonOperators = new Dictionary<string, string>
        {
            { "PropertyIsEqualTo", "=" },
            { "PropertyIsNotEqualTo", "≠" },
            { "PropertyIsLessThan", "<" },
            { "PropertyIsGreaterThan", ">" },
            { "PropertyIsLessThanOrEqualTo", "≤" },
            { "PropertyIsGreaterThanOrEqualTo", "≥" },
        };

        private static readonly Dictionary<string, string> ArithmeticOperators = new Dictionary<string, string>
        {
            { "Add", "+" }, { "Sub", "-" }, { "Mul", "*" }, { "Div", "/" },
        };

        public static string Format(XElement filter)
        {
            if (filter == null)
                return null;

            var ids = filter.Elements().Where(IsId).Select(Id).ToList();
            var operands = filter.Elements().Where(e => !IsId(e)).Select(e => Operator(e, false)).ToList();
            if (ids.Count > 0)
                operands.Add("ID = " + string.Join(", ", ids));

            return operands.Count == 0 ? null : string.Join(" AND ", operands);
        }

        private static string Operator(XElement element, bool nested)
        {
            string name = element.Name.LocalName;
            var children = element.Elements().ToList();

            string comparison;
            if (ComparisonOperators.TryGetValue(name, out comparison) && children.Count >= 2)
                return Expression(children[0]) + " " + comparison + " " + Expression(children[1]);

            switch (name)
            {
                case "And":
                case "Or":
                    var joined = string.Join(" " + name.ToUpperInvariant() + " ", children.Select(c => Operator(c, true)));
                    return nested && children.Count > 1 ? "(" + joined + ")" : joined;
                case "Not":
                    return "NOT (" + string.Join(" AND ", children.Select(c => Operator(c, false))) + ")";
                case "PropertyIsLike":
                    return children.Count >= 2 ? Expression(children[0]) + " LIKE " + Expression(children[1]) : name;
                case "PropertyIsNull":
                    return children.Count >= 1 ? Expression(children[0]) + " IS NULL" : name;
                case "PropertyIsNil":
                    return children.Count >= 1 ? Expression(children[0]) + " IS NIL" : name;
                case "PropertyIsBetween":
                    var lower = children.FirstOrDefault(c => c.Name.LocalName == "LowerBoundary");
                    var upper = children.FirstOrDefault(c => c.Name.LocalName == "UpperBoundary");
                    return children.Count >= 1
                        ? Expression(children[0]) + " BETWEEN " + Boundary(lower) + " AND " + Boundary(upper)
                        : name;
                case "FeatureId":
                case "GmlObjectId":
                case "ResourceId":
                    return "ID = " + Id(element);
                default:
                    // Spatial operators and other functions, show name and the property it applies to
                    var property = element.Descendants().FirstOrDefault(d => d.Name.LocalName == "PropertyName" || d.Name.LocalName == "ValueReference");
                    return property != null ? name + "(" + property.Value.Trim() + ")" : name;
            }
        }

        private static bool IsId(XElement element)
        {
            string name = element.Name.LocalName;
            return name == "FeatureId" || name == "GmlObjectId" || name == "ResourceId";
        }

        private static string Id(XElement element)
        {
            return element.Attributes().FirstOrDefault(a => a.Name.LocalName == "fid" || a.Name.LocalName == "id" || a.Name.LocalName == "rid")?.Value;
        }

        private static string Boundary(XElement boundary)
        {
            var expression = boundary?.Elements().FirstOrDefault();
            return expression != null ? Expression(expression) : "?";
        }

        private static string Expression(XElement element)
        {
            string name = element.Name.LocalName;
            var children = element.Elements().ToList();

            string arithmetic;
            if (ArithmeticOperators.TryGetValue(name, out arithmetic) && children.Count >= 2)
                return "(" + Expression(children[0]) + " " + arithmetic + " " + Expression(children[1]) + ")";

            switch (name)
            {
                case "PropertyName":
                case "ValueReference":
                    return element.Value.Trim();
                case "Literal":
                    return Literal(element.Value);
                case "Function":
                    return (string)element.Attribute("name") + "(" + string.Join(", ", children.Select(Expression)) + ")";
                default:
                    return element.Value.Trim();
            }
        }

        private static string Literal(string value)
        {
            double number;
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                return value.Trim();

            return "'" + value + "'";
        }
    }
}
