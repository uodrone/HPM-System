using System.Text.Json;

namespace HPM_System.EventService.Services.InterfacesImplementation
{
    public abstract class ClientBase
    {
        public readonly JsonSerializerOptions Options = 
            new()
            {
                PropertyNameCaseInsensitive = true
            };
    }
}
