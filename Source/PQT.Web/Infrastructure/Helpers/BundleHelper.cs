using System.Web.Optimization;

namespace PQT.Web.Infrastructure.Helpers
{
    public static class BundleHelper
    {
        public static Bundle Include(this Bundle bundle, IItemTransform transform, params string[] virtualPaths)
        {
            foreach (string virtualPath in virtualPaths)
                bundle.Include(virtualPath, transform);

            return bundle;
        }
    }
}
