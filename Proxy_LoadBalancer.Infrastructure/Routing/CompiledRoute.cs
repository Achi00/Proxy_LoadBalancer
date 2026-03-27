using Microsoft.AspNetCore.Http;
using Proxy_LoadBalancer.Infrastructure.Enums;
using Proxy_LoadBalancer.Infrastructure.Options;
using Proxy_LoadBalancer.Infrastructure.Options.Defaults;
using System.Text.RegularExpressions;

namespace Proxy_LoadBalancer.Infrastructure.Routing
{
    internal sealed class CompiledRoute
    {
        public RouteOption Route { get; }
        // max possible score, pre-computed
        public int BaseScore { get; }

        private readonly PathMatchType _matchType;
        private readonly Regex? _compiledRegex;
        private readonly HashSet<string>? _methods;

        public CompiledRoute(RouteOption route, ProxyDefaults defaults)
        {
            Route = route;

            var matchTypeStr = route.Match.PathMatch
                               ?? defaults.PathMatch
                               ?? "Prefix";

            _matchType = matchTypeStr switch
            {
                "Exact" => PathMatchType.Exact,
                "Prefix" => PathMatchType.Prefix,
                "Regex" => PathMatchType.Regex,
                _ => PathMatchType.Prefix
            };

            // pre-compile regex once
            if (_matchType == PathMatchType.Regex)
                _compiledRegex = new Regex(
                    route.Match.Path,
                    RegexOptions.Compiled | RegexOptions.IgnoreCase,
                    // timeout prevents ReDoS
                    TimeSpan.FromMilliseconds(100));

            // pre build for faster lookup
            _methods = route.Match.Methods is { Count: > 0 }
                ? new HashSet<string>(route.Match.Methods, StringComparer.OrdinalIgnoreCase)
                : null;

            BaseScore = _matchType switch
            {
                // set up by proprity
                PathMatchType.Exact => 1000 + route.Match.Path.Length,
                PathMatchType.Prefix => 500 + route.Match.Path.Length,
                PathMatchType.Regex => 100 + route.Match.Path.Length,
                _ => 0
            };
        }

        public int GetScore(PathString path, string method)
        {
            // hashset loopup
            if (_methods != null && !_methods.Contains(method))
            {
                return 0;
            }

            var pathValue = path.Value ?? "";

            return _matchType switch
            {
                // exact path match, case insensitive
                PathMatchType.Exact => pathValue.Equals(
                                            Route.Match.Path,
                                            StringComparison.OrdinalIgnoreCase)
                                        ? BaseScore : 0,
                // prefix only path match
                PathMatchType.Prefix => path.StartsWithSegments(Route.Match.Path)
                                        ? BaseScore : 0,
                // regex match
                PathMatchType.Regex => _compiledRegex!.IsMatch(pathValue)
                                        ? BaseScore : 0,
                _ => 0
            };
        }
    }
}
