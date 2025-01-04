
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
            CreateMap<PlayerModel, PlayerPacket>();
            CreateMap<CookieModel, CookiePacket>();
            CreateMap<PointModel, PointPacket>();
            CreateMap<TicketModel, TicketPacket>();
            CreateMap<ItemModel, ItemPacket>();
            CreateMap<KingdomStructureModel, KingdomStructurePacket>();
            CreateMap<KingdomDecoModel, KingdomDecoPacket>();
            CreateMap<KingdomMapModel, KingdomMapPacket>();

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

