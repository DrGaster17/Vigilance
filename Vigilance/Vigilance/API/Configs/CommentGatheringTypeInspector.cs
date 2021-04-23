using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace Vigilance.API.Configs
{
    public sealed class CommentGatheringTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector innerTypeDescriptor;

        public CommentGatheringTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            this.innerTypeDescriptor = innerTypeDescriptor ?? throw new ArgumentNullException("innerTypeDescriptor");
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return innerTypeDescriptor.GetProperties(type, container).Select(descriptor => new CommentsPropertyDescriptor(descriptor));
        }
    }
}