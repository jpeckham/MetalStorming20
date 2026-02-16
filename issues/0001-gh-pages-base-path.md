# GH Pages base path rewrite

GitHub Pages still relies on a `sed` replacement against `wwwroot/index.html` to update the `<base>` href during deploy. That workaround is brittle and easy to forget when we change the entry page. We should set the base path through publish properties instead so the build output is already correct.
