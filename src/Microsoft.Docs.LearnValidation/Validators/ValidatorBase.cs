// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.TripleCrown.Hierarchy.DataContract.Hierarchy;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Docs.LearnValidation
{
    public abstract class ValidatorBase
    {
        protected List<LegacyManifestItem> ManifestItems { get; }
        protected string BathPath { get; }
        public List<IValidateModel> Items { get; protected set; } = new List<IValidateModel>();

        public ValidatorBase(List<LegacyManifestItem> manifestItems, string basePath)
        {
            ManifestItems = manifestItems;
            BathPath = basePath;

            ExtractItems();
        }

        public abstract bool Validate(Dictionary<string, IValidateModel> fullItemsDict);
        protected abstract HierarchyItem GetHierarchyItem(ValidatorHierarchyItem validatorHierarchyItem, LegacyManifestItem manifestItem);

        protected virtual void ExtractItems()
        {
            if (ManifestItems == null) return;
            var items = new IValidateModel[ManifestItems.Count];
            Parallel.For(0, ManifestItems.Count, idx =>
            {
                var manifestItem = ManifestItems[idx];
                var path = Path.Combine(BathPath, manifestItem.Output.MetadataOutput.RelativePath);
                if (!File.Exists(path))
                {
                    path = manifestItem.Output.MetadataOutput.LinkToPath;
                }
                var validatorHierarchyItem = JsonConvert.DeserializeObject<ValidatorHierarchyItem>(File.ReadAllText(path));
                var hierarchyItem = GetHierarchyItem(validatorHierarchyItem, manifestItem);
                MergeToHierarchyItem(validatorHierarchyItem, hierarchyItem);
                items[idx] = (IValidateModel)hierarchyItem;
            });
            Items = items.ToList();
        }

        protected virtual void SetHierarchyData(IValidateModel item, ValidatorHierarchyItem validatorHierarchyItem, LegacyManifestItem manifestItem)
        {
            item.SourceRelativePath = manifestItem.SourceRelativePath!;
            item.AssetId = manifestItem.AssetId;
            item.MSDate = validatorHierarchyItem.MSDate;
            item.PublishUpdatedAt = validatorHierarchyItem.PublishUpdatedAt;
            item.PageKind = validatorHierarchyItem.PageKind;
            item.ServiceData = validatorHierarchyItem.ServiceData;
        }

        protected void MergeToHierarchyItem(ValidatorHierarchyItem validatorHierarchyItem, HierarchyItem hierarchyItem)
        {
            hierarchyItem.Abstract = validatorHierarchyItem.Abstract;
            hierarchyItem.Branch = validatorHierarchyItem.Branch;
            hierarchyItem.DepotName = validatorHierarchyItem.DepotName;
            hierarchyItem.Locale = validatorHierarchyItem.Locale;
            hierarchyItem.Points = validatorHierarchyItem.Points;
            hierarchyItem.Summary = validatorHierarchyItem.Summary;
            hierarchyItem.Url = validatorHierarchyItem.Url;
        }
    }
}
