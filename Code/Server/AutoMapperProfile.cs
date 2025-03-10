
using AutoMapper;
using Protocol;
using WebStudyServer.Manager;
using WebStudyServer.Model;
namespace WebStudyServer
{
    public class AutoMapperProfile : Profile
{
        public AutoMapperProfile()
        {
            CreateMap<PlayerModel, PlayerPacket>().ReverseMap();
            CreateMap<PlayerDetailModel, PlayerPacket>().ReverseMap();
            CreateMap<CookieModel, CookiePacket>().ReverseMap();
            CreateMap<PointModel, PointPacket>().ReverseMap();
            CreateMap<TicketModel, TicketPacket>().ReverseMap();
            CreateMap<ItemModel, ItemPacket>().ReverseMap();
            CreateMap<KingdomStructureModel, KingdomStructurePacket>().ReverseMap();
            CreateMap<KingdomDecoModel, KingdomDecoPacket>().ReverseMap();
            CreateMap<KingdomMapModel, KingdomMapPacket>().ReverseMap();
            CreateMap<WorldModel, WorldPacket>().ReverseMap();
            CreateMap<WorldStageModel, WorldStagePacket>().ReverseMap();

            CreateMap<ScheduleManager, SchedulePacket>()
                .ForMember(dest =>dest.Num, src=> src.MapFrom(src => src.Num))
                .ForMember(dest => dest.State, src => src.MapFrom(src => src.State))
                .ForMember(dest => dest.ActiveStartTime, src => src.MapFrom(src => src.ActiveStartTime))
                .ForMember(dest => dest.ActiveEndTime, src => src.MapFrom(src => src.ActiveEndTime))
                .ForMember(dest => dest.ContentStartTime, src => src.MapFrom(src => src.ContentStartTime))
                .ForMember(dest => dest.ContentEndTime, src => src.MapFrom(src => src.ContentEndTime))
                ;

            // Example
            //CreateMap<UpdateRequest, User>()
            //    .ForAllMembers(x => x.Condition(
            //        (src, dest, prop) =>
            //        {
            //            // ignore both null & empty string properties
            //            if (prop == null) return false;
            //            if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop)) return false;

            //            // ignore null role
            //            if (x.DestinationMember.Name == "Role" && src.Role == null) return false;

            //            return true;
            //        }
            //    ));
        }
    }
}

