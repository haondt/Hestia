using Haondt.Core.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Hestia.ModelBinders
{
    public class OptionalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType.IsGenericType && context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(Optional<>))
                return new OptionalModelBinder();

            return null;
        }
    }
}
