﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public sealed class ConverterContractResolver : CamelCasePropertyNamesContractResolver
    {
        private readonly JsonConverter[] converters;
        private readonly object lockObject = new object();
        private Dictionary<Type, JsonConverter> converterCache = new Dictionary<Type, JsonConverter>();

        public ConverterContractResolver(params JsonConverter[] converters)
        {
            NamingStrategy = new CamelCaseNamingStrategy(false, true);

            this.converters = converters;

            foreach (var converter in converters)
            {
                if (converter is ISupportedTypes supportedTypes)
                {
                    foreach (var type in supportedTypes.SupportedTypes)
                    {
                        converterCache[type] = converter;
                    }
                }
            }
        }

        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            var result = base.ResolveContractConverter(objectType);

            if (result != null)
            {
                return result;
            }

            var cache = converterCache;

            if (cache == null || !cache.TryGetValue(objectType, out result))
            {
                foreach (var converter in converters)
                {
                    if (converter.CanConvert(objectType))
                    {
                        result = converter;
                    }
                }

                lock (lockObject)
                {
                    cache = converterCache;

                    var updatedCache = (cache != null)
                        ? new Dictionary<Type, JsonConverter>(cache)
                        : new Dictionary<Type, JsonConverter>();
                    updatedCache[objectType] = result;

                    converterCache = updatedCache;
                }
            }

            return result;
        }
    }
}
