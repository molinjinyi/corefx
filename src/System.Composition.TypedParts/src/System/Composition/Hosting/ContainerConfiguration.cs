﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Debugging;
using System.Composition.Hosting.Core;
using System.Composition.TypedParts;
using System.Composition.TypedParts.Util;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace System.Composition.Hosting
{
    /// <summary>
    /// Configures and constructs a lightweight container.
    /// </summary>
    [DebuggerTypeProxy(typeof(ContainerConfigurationDebuggerProxy))]
    public class ContainerConfiguration
    {
        private AttributedModelProvider _defaultAttributeContext;
        private readonly IList<ExportDescriptorProvider> _addedSources = new List<ExportDescriptorProvider>();
        private readonly IList<Tuple<IEnumerable<Type>, AttributedModelProvider>> _types = new List<Tuple<IEnumerable<Type>, AttributedModelProvider>>();

        /// <summary>
        /// Create the container. The value returned from this method provides
        /// the exports in the container, as well as a means to dispose the container.
        /// </summary>
        /// <returns>The container.</returns>
        public CompositionHost CreateContainer()
        {
            var providers = _addedSources.ToList();

            foreach (var typeSet in _types)
            {
                var ac = typeSet.Item2 ?? _defaultAttributeContext ?? new DirectAttributeContext();

                providers.Add(new TypedPartExportDescriptorProvider(typeSet.Item1, ac));
            }

            return CompositionHost.CreateCompositionHost(providers.ToArray());
        }

        /// <summary>
        /// Add an export descriptor provider to the container.
        /// </summary>
        /// <param name="exportDescriptorProvider">An export descriptor provider.</param>
        /// <returns>A configuration object allowing configuration to continue.</returns>
        public ContainerConfiguration WithProvider(ExportDescriptorProvider exportDescriptorProvider)
        {
            if (exportDescriptorProvider == null) throw new ArgumentNullException("ExportDescriptorProvider");
            _addedSources.Add(exportDescriptorProvider);
            return this;
        }

        /// <summary>
        /// Add conventions defined using a <see cref="AttributedModelProvider"/> to the container.
        /// These will be used as the default conventions; types and assemblies added with a
        /// specific convention will use their own.
        /// </summary>
        /// <param name="conventions"></param>
        /// <returns>A configuration object allowing configuration to continue.</returns>
        public ContainerConfiguration WithDefaultConventions(AttributedModelProvider conventions)
        {
            if (conventions == null) throw new ArgumentNullException("conventions");

            if (_defaultAttributeContext != null)
                throw new InvalidOperationException(System.Composition.Properties.Resources.ContainerConfiguration_DefaultConventionSet);

            _defaultAttributeContext = conventions;
            return this;
        }

        /// <summary>
        /// Add a part type to the container. If the part type does not have any exports it
        /// will be ignored.
        /// </summary>
        /// <param name="partType">The part type.</param>
        /// <returns>A configuration object allowing configuration to continue.</returns>
        public ContainerConfiguration WithPart(Type partType)
        {
            return WithPart(partType, null);
        }

        /// <summary>
        /// Add a part type to the container. If the part type does not have any exports it
        /// will be ignored.
        /// </summary>
        /// <param name="partType">The part type.</param>
        /// <param name="conventions">Conventions represented by a <see cref="AttributedModelProvider"/>, or null.</param>
        /// <returns>A configuration object allowing configuration to continue.</returns>
        public ContainerConfiguration WithPart(Type partType, AttributedModelProvider conventions)
        {
            if (partType == null) throw new ArgumentNullException("partType");
            return WithParts(new[] { partType }, conventions);
        }

        /// <summary>
        /// Add a part type to the container. If the part type does not have any exports it
        /// will be ignored.
        /// </summary>
        /// <typeparam name="TPart">The part type.</typeparam>
        /// <returns>A configuration object allowing configuration to continue.</returns>
        public ContainerConfiguration WithPart<TPart>()
        {
            return WithPart<TPart>(null);
        }

        /// <summary>
        /// Add a part type to the container. If the part type does not have any exports it
        /// will be ignored.
        /// </summary>
        /// <typeparam name="TPart">The part type.</typeparam>
        /// <param name="conventions">Conventions represented by a <see cref="AttributedModelProvider"/>, or null.</param>
        /// <returns>A configuration object allowing configuration to continue.</returns>
        public ContainerConfiguration WithPart<TPart>(AttributedModelProvider conventions)
        {
            return WithPart(typeof(TPart), conventions);
        }

        /// <summary>
        /// Add part types to the container. If a part type does not have any exports it
        /// will be ignored.
        /// </summary>
        /// <param name="partTypes">The part types.</param>
        /// <returns>A configuration object allowing configuration to continue.</returns>
        public ContainerConfiguration WithParts(params Type[] partTypes)
        {
            return WithParts((IEnumerable<Type>)partTypes);
        }

        /// <summary>
        /// Add part types to the container. If a part type does not have any exports it
        /// will be ignored.
        /// </summary>
        /// <param name="partTypes">The part types.</param>
        /// <returns>A configuration object allowing configuration to continue.</returns>
        public ContainerConfiguration WithParts(IEnumerable<Type> partTypes)
        {
            return WithParts(partTypes, null);
        }

        /// <summary>
        /// Add part types to the container. If a part type does not have any exports it
        /// will be ignored.
        /// </summary>
        /// <param name="partTypes">The part types.</param>
        /// <param name="conventions">Conventions represented by a <see cref="AttributedModelProvider"/>, or null.</param>
        /// <returns>A configuration object allowing configuration to continue.</returns>
        public ContainerConfiguration WithParts(IEnumerable<Type> partTypes, AttributedModelProvider conventions)
        {
            if (partTypes == null) throw new ArgumentNullException("partTypes");
            _types.Add(Tuple.Create(partTypes, conventions));
            return this;
        }

        /// <summary>
        /// Add part types from an assembly to the container. If a part type does not have any exports it
        /// will be ignored.
        /// </summary>
        /// <param name="assembly">The assembly from which to add part types.</param>
        /// <returns>A configuration object allowing configuration to continue.</returns>
        public ContainerConfiguration WithAssembly(Assembly assembly)
        {
            return WithAssembly(assembly, null);
        }

        /// <summary>
        /// Add part types from an assembly to the container. If a part type does not have any exports it
        /// will be ignored.
        /// </summary>
        /// <param name="assembly">The assembly from which to add part types.</param>
        /// <param name="conventions">Conventions represented by a <see cref="AttributedModelProvider"/>, or null.</param>
        /// <returns>A configuration object allowing configuration to continue.</returns>
        public ContainerConfiguration WithAssembly(Assembly assembly, AttributedModelProvider conventions)
        {
            return WithAssemblies(new[] { assembly }, conventions);
        }

        /// <summary>
        /// Add part types from a list of assemblies to the container. If a part type does not have any exports it
        /// will be ignored.
        /// </summary>
        /// <param name="assemblies">Assemblies containing part types.</param>
        /// <returns>A configuration object allowing configuration to continue.</returns>
        public ContainerConfiguration WithAssemblies(IEnumerable<Assembly> assemblies)
        {
            return WithAssemblies(assemblies, null);
        }

        /// <summary>
        /// Add part types from a list of assemblies to the container. If a part type does not have any exports it
        /// will be ignored.
        /// </summary>
        /// <param name="assemblies">Assemblies containing part types.</param>
        /// <param name="conventions">Conventions represented by a <see cref="AttributedModelProvider"/>, or null.</param>
        /// <returns>A configuration object allowing configuration to continue.</returns>
        public ContainerConfiguration WithAssemblies(IEnumerable<Assembly> assemblies, AttributedModelProvider conventions)
        {
            if (assemblies == null) throw new ArgumentNullException("assemblies");
            return WithParts(assemblies.SelectMany(a => a.DefinedTypes.Select(dt => dt.AsType())), conventions);
        }

        internal ExportDescriptorProvider[] DebugGetAddedExportDescriptorProviders()
        {
            return _addedSources.ToArray();
        }

        internal Tuple<IEnumerable<Type>, AttributedModelProvider>[] DebugGetRegisteredTypes()
        {
            return _types.ToArray();
        }

        internal AttributedModelProvider DebugGetDefaultAttributeContext()
        {
            return _defaultAttributeContext;
        }
    }
}
