using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class ReactorType
    {
        private string name;
        private string imgPath;

        public string Name
        {
            get => name;
            set
            {
                if(value != name)
                {
                    name = value;
                }
            }
        }
        public string ImgPath
        {
            get => imgPath;
            set
            {
                if(value != imgPath)
                {
                    imgPath = value;
                }
            }
        }
    }
}
