# GH Pages SPA fallback

Client-side routes will 404 on GitHub Pages because there is no fallback document. We need a 404 redirect that replays the original path and a small hook in `index.html` to restore the URL before Blazor boots.
