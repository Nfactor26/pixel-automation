using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Extensions
{
    public class JsonModelBinder : IModelBinder
    {
        private readonly JsonSerializerOptions _options;

        public JsonModelBinder(IOptions<JsonSerializerOptions> options) =>
            _options = options.Value;


        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext is null)
                throw new ArgumentNullException(nameof(bindingContext));

            // Test if a value is received
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            // Get the json serialized value as string
            string serialized = valueProviderResult.FirstValue;

            // Return a successful binding for empty strings or nulls
            if (String.IsNullOrEmpty(serialized))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            // Deserialize json string using custom json options defined in startup, if available
            object deserialized = JsonSerializer.Deserialize(serialized, bindingContext.ModelType, _options);

            // Run data annotation validation to validate properties and fields on deserialized model
            var validationResultProps = from property in TypeDescriptor.GetProperties(deserialized).Cast<PropertyDescriptor>()
                                        from attribute in property.Attributes.OfType<ValidationAttribute>()
                                        where !attribute.IsValid(property.GetValue(deserialized))
                                        select new
                                        {
                                            Member = property.Name,
                                            ErrorMessage = attribute.FormatErrorMessage(String.Empty)
                                        };

            var validationResultFields = from field in TypeDescriptor.GetReflectionType(deserialized).GetFields().Cast<FieldInfo>()
                                         from attribute in field.GetCustomAttributes<ValidationAttribute>()
                                         where !attribute.IsValid(field.GetValue(deserialized))
                                         select new
                                         {
                                             Member = field.Name,
                                             ErrorMessage = attribute.FormatErrorMessage(String.Empty)
                                         };

            // Add the validation results to the model state
            var errors = validationResultFields.Concat(validationResultProps);
            foreach (var validationResultItem in errors)
                bindingContext.ModelState.AddModelError(validationResultItem.Member, validationResultItem.ErrorMessage);

            // Set successful binding result
            bindingContext.Result = ModelBindingResult.Success(deserialized);

            return Task.CompletedTask;
        }
    }
}
