
using AutoMapper;
using Protocol;
using WebStudyServer.Model;
namespace WebStudyServer
{
    public class AutoMapperProfile : Profile
{
        public AutoMapperProfile()
        {
            // CreateRequest -> User
            CreateMap<PlayerModel, PlayerPacket>().ReverseMap();
            CreateMap<PlayerDetailModel, PlayerPacket>().ReverseMap();
            CreateMap<CookieModel, CookiePacket>().ReverseMap();
            CreateMap<PointModel, PointPacket>().ReverseMap();
            CreateMap<TicketModel, TicketPacket>().ReverseMap();
            CreateMap<ItemModel, ItemPacket>().ReverseMap();
            CreateMap<KingdomStructureModel, KingdomStructurePacket>().ReverseMap();
            CreateMap<KingdomDecoModel, KingdomDecoPacket>().ReverseMap();
            CreateMap<KingdomMapModel, KingdomMapPacket>().ReverseMap();

            /*            // UpdateRequest -> User
                        CreateMap<UpdateRequest, User>()
                            .ForAllMembers(x => x.Condition(
                                (src, dest, prop) =>
                                {
                                    // ignore both null & empty string properties
                                    if (prop == null) return false;
                                    if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop)) return false;

                                    // ignore null role
                                    if (x.DestinationMember.Name == "Role" && src.Role == null) return false;

                                    return true;
                                }
                            ));*/
        }
    }
}

