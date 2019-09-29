using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace PictureApp.API.Dtos.ModelBinders
{
    public class FileModelBinder : IModelBinder
    {
        private readonly MvcJsonOptions _options;

        public FileModelBinder(IOptions<MvcJsonOptions> options)
        {
            _options = options.Value;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (!(valueProviderResult != ValueProviderResult.None))
                return Task.CompletedTask;
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
            string firstValue = valueProviderResult.FirstValue;
            object model = _options?.SerializerSettings == null
                ? JsonConvert.DeserializeObject(firstValue, bindingContext.ModelType)
                : JsonConvert.DeserializeObject(firstValue, bindingContext.ModelType, _options.SerializerSettings);

            var property = bindingContext.ModelMetadata.Properties.ToList().SingleOrDefault(x =>
                x.ModelType == typeof(ICollection<IFormFile>));

            if (property != null)
            {
                ICollection<IFormFile> files = new List<IFormFile>();

                bindingContext.HttpContext.Request.Form.Files.ToList().ForEach(x =>
                {
                    files.Add(x);
                });

                property.PropertySetter(model, files);
            }

            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }
}
