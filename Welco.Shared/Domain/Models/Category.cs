using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class Category : BaseEntity<Guid>
    {
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageName { get; set; }
        public Guid? ParentCategoryId { get; set; }

        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        public static Category Create(
            string nameEn,
            string nameAr,
            string? description,
            string? imageName,
            Guid? parentCategoryId,
            string createdBy)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                NameEn = nameEn,
                NameAr = nameAr,
                Description = description,
                ImageName = imageName,
                ParentCategoryId = parentCategoryId
            };
            category.MarkAsCreated(createdBy);
            return category;
        }

        public void Update(
            string nameEn,
            string nameAr,
            string? description,
            string? imageName,
            Guid? parentCategoryId,
            string updatedBy)
        {
            NameEn = nameEn.Trim();
            NameAr = nameAr.Trim();
            if (description != null) Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            if (imageName != null) ImageName = string.IsNullOrWhiteSpace(imageName) ? null : imageName.Trim();
            ParentCategoryId = parentCategoryId;
            MarkAsUpdated(updatedBy);
        }
    }
}
