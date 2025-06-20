using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class PICDTO
    {
        public string picKey { get; set; }
        public string picName { get; set; }

        public PICDTO(string picKey, string picName)
        {
            this.picKey = picKey;
            this.picName = picName;
        }
    }

    public class ClassificationDTO
    {
        public string classificationKey { get; set; }
        public string classificationName { get; set; }
        public string imageURL { get; set; }

        public ClassificationDTO(string classificationKey, string classificationName, string imageURL)
        {
            this.classificationKey = classificationKey;
            this.classificationName = classificationName;
            this.imageURL = imageURL;
        }
    }
}