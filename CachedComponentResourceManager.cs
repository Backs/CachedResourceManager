using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Resources;
using System.Security.Permissions;

namespace CachedResourceManager
{
    [HostProtection(SecurityAction.LinkDemand, SharedState = true)]
    public sealed class CachedComponentResourceManager : ResourceManager
    {
        private static readonly Dictionary<Type, Dictionary<string, Action<object>>> Setters =
            new Dictionary<Type, Dictionary<string, Action<object>>>();

        private static readonly List<Type> ResourceValuesToDeepClone = new List<Type>();

        /// <summary>
        /// Add types that must to be deep cloned, because objects change resources after load
        /// </summary>
        /// <example>CachedComponentResourceManager.AddResourceValuesToDeepClone(typeof(TableLayoutSettings))</example>
        public static void AddResourceValuesToDeepClone(Type resourceType)
        {
            ResourceValuesToDeepClone.Add(resourceType);
        }

        private static readonly Dictionary<Type, Dictionary<string, object>> Objects =
            new Dictionary<Type, Dictionary<string, object>>();

        private static CultureInfo _neutralResourcesCulture;

        private readonly Type _type;

        private CultureInfo NeutralResourcesCulture
        {
            get
            {
                if (_neutralResourcesCulture == null && MainAssembly != null)
                    _neutralResourcesCulture = GetNeutralResourcesLanguage(MainAssembly);
                return _neutralResourcesCulture;
            }
        }

        public CachedComponentResourceManager(Type type) : base(type)
        {
            _type = type;
        }

        public void ApplyResources(object value, string objectName)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            var culture = CultureInfo.CurrentUICulture;

            if (!Setters.ContainsKey(_type))
            {
                Setters[_type] = new Dictionary<string, Action<object>>();
            }

            var setterCompiled = Setters[_type].TryFindValue(objectName);

            if (setterCompiled == null)
            {
                var valueProperties = new Lazy<PropertyDescriptorCollection>(() => TypeDescriptor.GetProperties(value));

                var setters = new List<BinaryExpression>();
                var targetExp = Expression.Parameter(typeof(object), "x");

                Dictionary<string, object> list = GetResources(culture);

                foreach (var keyValuePair in list)
                {
                    var key = keyValuePair.Key;

                    if (string.Compare(key, 0, objectName, 0, objectName.Length, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        continue;
                    }
                    var length = objectName.Length;
                    if (key.Length > length && key[length] == '.')
                    {
                        var name = key.Substring(length + 1);

                        var propertyDescriptor = valueProperties.Value.Find(name, IgnoreCase);

                        if (propertyDescriptor != null && !propertyDescriptor.IsReadOnly &&
                            (keyValuePair.Value == null ||
                             propertyDescriptor.PropertyType.IsInstanceOfType(keyValuePair.Value)))
                        {

                            var propertyExp = Expression.Property(Expression.Convert(targetExp, value.GetType()), name);

                            Expression propertyValue = keyValuePair.Value != null
                                    ? Expression.Constant(keyValuePair.Value, keyValuePair.Value.GetType())
                                    : Expression.Constant(null);
                            if (keyValuePair.Value != null &&
                                ResourceValuesToDeepClone.Contains(keyValuePair.Value.GetType()))
                            {
                                var methodInfo = ReflectionExtensions.GetMethodInfo<object>(t => t.DeepClone());

                                propertyValue = Expression.Call(methodInfo, propertyValue);
                            }

                            setters.Add(Expression.Assign(propertyExp, Expression.Convert(propertyValue, propertyExp.Type)));
                        }
                    }
                }

                var settersBlock = Expression.Block(setters);
                setterCompiled = Setters[_type][objectName] = Expression.Lambda<Action<object>>(settersBlock, targetExp).Compile();
            }

            setterCompiled.Invoke(value);

        }

        public override object GetObject(string name)
        {
            if (!Objects.ContainsKey(_type))
            {
                Objects[_type] = new Dictionary<string, object>();
            }

            var result = Objects[_type].TryFindValue(name);

            if (result == null)
            {
                result = Objects[_type][name] = base.GetObject(name);
            }

            return result;
        }

        private Dictionary<string, object> GetResources(CultureInfo culture)
        {
            Dictionary<string, object> resources;

            if (culture.Equals(CultureInfo.InvariantCulture) || culture.Equals(NeutralResourcesCulture))
            {
                resources = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                resources = GetResources(culture.Parent);
            }

            var resourceSet = GetResourceSet(culture, true, true);
            if (resourceSet != null)
            {
                foreach (DictionaryEntry dictionaryEntry in resourceSet)
                    resources[(string)dictionaryEntry.Key] = dictionaryEntry.Value;
            }
            return resources;
        }
    }
}
