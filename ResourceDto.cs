using System;
using UnityEngine;

namespace ResourceMapper {
    [Serializable]
    public class ResourceDto {
        private string path;
        private long count;

        public string Path => path;

        public long Count => count;

        public ResourceDto(string path) {
            this.path = path;
            count = 0;
        }

        private ResourceDto(string path, long count) {
            this.path = path;
            this.count = count;
        }

        public ResourceDto CreateNewVersion(string path) {
            return new ResourceDto(path, count + 1);
        }
    }
}