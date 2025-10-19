namespace Catalog.Contracts.DTOs
{

    public record CategoryDto(int Id, string Name, string Image, int? ParentCategoryId);
    public record CreateCategoryDto(string Name, string Image, int? ParentCategoryId);
    public record UpdateCategoryDto(int Id, string Name, string Image, int? ParentCategoryId);

}
