using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectTrack
{
    class ARFFMaker
    {
        List<String> pNames;

        public ARFFMaker()
        {
            pNames = new List<string>();
        }

        public void addProp(String name)
        {
            if(name.StartsWith("distance"))
            {
                for(int i = 0; i < 180; i++) 
                {
                    pNames.Add(name + i);
                }
            }
            else 
            {
                pNames.Add(name);
            }
        }

        public String getARFF(List<Stride> strideList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (String name in pNames)
            {
                sb.Append("@attribute " + name + " numeric");
                sb.Append("\n");
            }

            sb.Append("@data \n");

            foreach (Stride s in strideList)
            {
                foreach (String name in pNames)
                {
                    if (name.StartsWith("distance")) 
                    {
                        if(name.EndsWith("0")) {
                            for(int i = 0; i < 180; i++) {
                                sb.Append(
                                    ((double[])(s.GetType().GetProperty(name).GetValue(s, null)))[i]);
                            }
                        }
                        else {
                            continue;
                        }   
                    }
                    var value = s.GetType().GetProperty(name).GetValue(s, null);
                    sb.Append(value);
                    sb.Append(", ");
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }

    }
}
