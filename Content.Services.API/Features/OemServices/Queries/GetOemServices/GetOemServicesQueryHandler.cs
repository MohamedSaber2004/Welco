using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Localization;
using Welco.Shared.Results;
namespace Content.Services.API.Features.OemServices.Queries.GetOemServices
{
    public class GetOemServicesQueryHandler : IRequestHandler<GetOemServicesQuery, Result<List<OemServiceDto>>>
    {
        // Static catalog mirrors the 5 OEM services showcased on the storefront.
        private static readonly List<OemServiceDto> Services = new()
        {
            new() { Id = "oem-1", Title = "Private Labeling & Laser Marking", TitleAr = "وضع العلامة التجارية الخاصة ووسم الليزر", Description = "Direct fiber-laser marking for UDI, GS1 DataMatrix barcodes, custom hospital initials, and catalog SKU branding.", DescriptionAr = "وسم ألياف الليزر المباشر لـ UDI ورموز باركود GS1 والحروف المخصصة للمستشفيات ورمز المنتج.", Icon = "label" },
            new() { Id = "oem-2", Title = "Custom Dimensions & Precision Forging", TitleAr = "أبعاد مخصصة والحدادة الدقيقة", Description = "Tailored surgical tool dimensions, custom tip geometry, and modified working ends per surgeon CAD specifications.", DescriptionAr = "تعديل قياسات الأدوات الجراحية وهندسة الأطراف المخصصة وفقًا لمواصفات ومخططات الجراحين.", Icon = "ruler" },
            new() { Id = "oem-3", Title = "Optical Surface Finishing & Passivation", TitleAr = "التشطيب البصري للسطح والتخميل الكيميائي", Description = "Electropolishing, glare-reducing satin finishes, tungsten carbide jaw inserts, and ASTM A967 passivation.", DescriptionAr = "التلميع الكهربائي، والتشطيب المطفي المضاد للانعكاس، وفكوك كربيد التنجستن، وتخميل ASTM A967.", Icon = "scan" },
            new() { Id = "oem-4", Title = "Sterile Barrier Packaging & Kit Assembly", TitleAr = "تغليف الحواجز المعقمة وتجميع الأطقم", Description = "Cleanroom ISO Class 7 pouching, custom tray layouts, and validation for steam autoclave and EtO sterilization.", DescriptionAr = "التعبئة في غرف نظيفة ISO Class 7، وتصميم صواني الأدوات المخصصة، والتحقق للتعقيم بالبخار أو أكسيد الإيثيلين.", Icon = "package" },
            new() { Id = "oem-5", Title = "Regulatory Dossier & MDR Class IIa Filing", TitleAr = "الملفات التنظيمية واعتماد MDR Class IIa", Description = "Complete technical documentation, biocompatibility test results, and CE MDR compliance dossiers for overseas registration.", DescriptionAr = "توثيق تقني شامل، ونتائج اختبارات التوافق الحيوي، وملفات الامتثال لمعايير CE MDR للتسجيل الدولي.", Icon = "flag" },
        };
        public Task<Result<List<OemServiceDto>>> Handle(GetOemServicesQuery request, CancellationToken cancellationToken)
            => Task.FromResult(Result<List<OemServiceDto>>.Success(Services, LocalizationKeys.Content.ListFetched));
    }
}
