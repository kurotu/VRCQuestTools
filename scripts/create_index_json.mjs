import got from 'got';
import fs from 'fs';

const repo = process.argv[2];
const packageJson = JSON.parse(fs.readFileSync(process.argv[3], 'utf-8'));

(async () => {
    const releases = await got(`https://api.github.com/repos/${repo}/releases`).json();
    const vpm_releases = releases.filter(r => isVpmRelease(r));
    const versionDatas = await Promise.all(vpm_releases.map(r => getVersionData(r)));
    const versions = {};
    for (let v of versionDatas) {
        v.url += '?';
        versions[v.version] = v;
    }
    const list = {
        name: packageJson.displayName,
        author: packageJson.author.name,
        packages: {},
    };
    list.packages[packageJson.name] = {
        versions: versions
    };
    console.log(JSON.stringify(list, null, '  '));
})();

function isVpmRelease(release) {
    if (!release.assets.find(asset => asset.name === 'package.json')) {
        return false;
    }
    const version = release.tag_name.slice(1);
    if (!release.assets.find(asset => asset.name === `${packageJson.name}-${version}.zip`)) {
        return false;
    }
    return true;
}

async function getVersionData(release) {
    const json = await got(release.assets.find(a => a.name === 'package.json').browser_download_url).json();
    return json;
}
