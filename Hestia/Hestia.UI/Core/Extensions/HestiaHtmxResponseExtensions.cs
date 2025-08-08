using Haondt.Web.Core.Http;
using Newtonsoft.Json;

namespace Hestia.UI.Core.Extensions
{
    public static class HestiaHtmxResponseExtensions
    {
        public const string HX_TRIGGER_AFTER_SETTLE = "HX-Trigger-After-Settle";
        public const string HX_TRIGGER_AFTER_SWAP = "HX-Trigger-After-Swap";

        public static IResponseData HxTriggerAfterSettle(this IResponseData responseData,
            string @event,
            string? body = null,
            string? target = null)
        {
            return HxTriggerAfterSettle(responseData, @event, new Dictionary<string, string> { { "value", body ?? "" } }, target);
        }

        public static IResponseData HxTriggerAfterSettle(this IResponseData responseData,
            string @event,
            Dictionary<string, string> body,
            string? target = null)
        {
            foreach (var key in body.Keys)
                if ("target".Equals(key, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException($"Cannot use key {key}, \"target\" is a reserved keyword");

            return responseData.ReplaceHeader(HX_TRIGGER_AFTER_SETTLE, existing =>
            {
                Dictionary<string, Dictionary<string, string>> existingPayload;
                if (existing.Length > 0 && existing[0] != null)
                {
                    try
                    {
                        existingPayload = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(existing[0]!)
                            ?? new();
                    }
                    catch
                    {
                        existingPayload = new();
                    }
                }
                else
                    existingPayload = new();

                existingPayload[@event] = body ?? new();
                if (target != null)
                    existingPayload[@event]["target"] = target;

                return JsonConvert.SerializeObject(existingPayload);
            });
        }

        public static IResponseData HxTriggerAfterSwap(this IResponseData responseData,
            string @event,
            string? body = null,
            string? target = null)
        {
            return HxTriggerAfterSwap(responseData, @event, new Dictionary<string, string> { { "value", body ?? "" } }, target);
        }

        public static IResponseData HxTriggerAfterSwap(this IResponseData responseData,
            string @event,
            Dictionary<string, string> body,
            string? target = null)
        {
            foreach (var key in body.Keys)
                if ("target".Equals(key, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException($"Cannot use key {key}, \"target\" is a reserved keyword");

            return responseData.ReplaceHeader(HX_TRIGGER_AFTER_SWAP, existing =>
            {
                Dictionary<string, Dictionary<string, string>> existingPayload;
                if (existing.Length > 0 && existing[0] != null)
                {
                    try
                    {
                        existingPayload = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(existing[0]!)
                            ?? new();
                    }
                    catch
                    {
                        existingPayload = new();
                    }
                }
                else
                    existingPayload = new();

                existingPayload[@event] = body ?? new();
                if (target != null)
                    existingPayload[@event]["target"] = target;

                return JsonConvert.SerializeObject(existingPayload);
            });
        }
    }
}
