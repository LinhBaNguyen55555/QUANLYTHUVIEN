namespace QUANLYTHUVIEN.Utilities
{
    public class Function
    {
        public static string TitleSlugGenerationAlias(String title)
        {
            return SlugGenerator.SlugGenerator.GenerateSlug(title);
        }
    }
}
