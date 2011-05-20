using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpShare.Storage {
    public static class StorageExtensions {
        public static IEnumerable<IStorageItem> ListAllContents(this IStorageContainer container) {
            foreach (IStorageItem item in container.ListContents()) {
                yield return item;

                IStorageContainer childContainer = item as IStorageContainer;

                if (childContainer != null) {
                    foreach (IStorageItem itemItem in childContainer.ListAllContents()) {
                        yield return itemItem;
                    }
                }
            }
        }
    }
}
