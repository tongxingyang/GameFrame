namespace GameFrame.Update.Version
{
    public class Version
    {
        /// <summary>
        /// 主版本号
        /// </summary>
        private int master = 1;
        /// <summary>
        /// 次版本号
        /// </summary>
        private int minor = 0;
        /// <summary>
        /// 修订版本号
        /// </summary>
        private int revised = 0;

        public int Master
        {
            get { return master; }
        }
        
        public int Minor
        {
            get { return minor; }
        }


        public int Revised
        {
            get { return revised; }
        }

        public Version(string ver)
        {
            string[] arr = ver.Split('.');
            master = int.Parse(arr[0]);
            minor = int.Parse(arr[1]);
            revised = int.Parse(arr[2]);
        }

        public static CompareResult CompareVersion(Version a,Version b,bool isComRevised = true)
        {
            if (a.master > b.master)
            {
                return CompareResult.Greater;
            }
            if(a.master < b.master)
            {
                return CompareResult.Less;
            }
            if (a.minor > b.minor)
            {
                return CompareResult.Greater;
            }
            if(a.minor < b.minor)
            {
                return CompareResult.Less;
            }
            if (isComRevised)
            {
                if (a.revised > b.revised)
                {
                    return CompareResult.Greater;
                }
                if(a.revised < b.revised)
                {
                    return CompareResult.Less;
                }
            }
            
            return CompareResult.Equal;
        }
    }
    
}