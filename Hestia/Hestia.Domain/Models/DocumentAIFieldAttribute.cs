namespace Hestia.Domain.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DocumentAIFieldAttribute : Attribute
    {
        public string FieldName { get; }

        public DocumentAIFieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}