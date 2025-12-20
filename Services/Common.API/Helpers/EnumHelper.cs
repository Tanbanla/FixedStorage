using System.Reflection;
#nullable disable

namespace BIVN.FixedStorage.Services.Common.API.Helpers
{
    public static class EnumHelper<T> where T : struct, System.Enum
    {
        public static IList<T> GetValues(System.Enum value)
        {
            var enumValues = new List<T>();

            foreach (FieldInfo fi in value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                enumValues.Add((T)System.Enum.Parse(value.GetType(), fi.Name, false));
            }
            return enumValues;
        }

        public static T Parse(string value)
        {
            return (T)System.Enum.Parse(typeof(T), value, true);
        }

        public static bool IsValidValueFromName(string name)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();

            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DisplayAttribute)) as DisplayAttribute;
                if (attribute != null)
                {
                    if (attribute.Name.Trim().ToLower() == name.Trim().ToLower())
                    {
                        return true;
                    }
                }
                else
                {
                    if (field.Name.Trim().ToLower() == name.Trim().ToLower())
                        return true;
                }
            }
            return false;
        }

        public static T GetValueFromName(string name)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();

            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DisplayAttribute)) as DisplayAttribute;
                if (attribute != null)
                {
                    if (attribute.Name.Trim().ToLower() == name.Trim().ToLower())
                    {
                        return (T)field.GetValue(null);
                    }
                }
                else
                {
                    if (field.Name.Trim().ToLower() == name.Trim().ToLower())
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentOutOfRangeException("name");
        }

        public static IList<string> GetNames(System.Enum value)
        {
            return value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public).Select(fi => fi.Name).ToList();
        }

        public static IList<string> GetDisplayValues(System.Enum value)
        {
            return GetNames(value).Select(obj => GetDisplayValue(Parse(obj))).ToList();
        }

        private static string LookupResource(Type resourceManagerProvider, string resourceKey)
        {
            var resourceKeyProperty = resourceManagerProvider.GetProperty(resourceKey,
                BindingFlags.Static | BindingFlags.Public, null, typeof(string),
                new Type[0], null);
            if (resourceKeyProperty != null)
            {
                return (string)resourceKeyProperty.GetMethod.Invoke(null, null);
            }

            return resourceKey; // Fallback with the key name
        }

        public static string GetDisplayValue(T value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(
                typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes[0].ResourceType != null)
                return LookupResource(descriptionAttributes[0].ResourceType, descriptionAttributes[0].Name);

            if (descriptionAttributes == null) return string.Empty;
            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();
        }

        public static T GetEnumFromDisplay(string displayName)
        {
            var type = typeof(T);
            foreach (var field in type.GetFields())
            {
                var displayAttr = field.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                if (displayAttr == null)
                {
                    // Thử lấy Display từ Microsoft.OpenApi.Attributes nếu DisplayAttribute không tồn tại
                    var openApiDisplayAttr = field.GetCustomAttribute<Microsoft.OpenApi.Attributes.DisplayAttribute>();
                    if (openApiDisplayAttr != null && openApiDisplayAttr.Name == displayName)
                    {
                        return (T)field.GetValue(null);
                    }
                }
                else if (displayAttr.Name == displayName)
                {
                    return (T)field.GetValue(null);
                }
            }
            return default;
        }

    }

    public static class EnumHelper
    {
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>,
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example>string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;</DescriptionAttribute></example>
        public static T GetAttributeOfType<T>(this System.Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }
    }
}
