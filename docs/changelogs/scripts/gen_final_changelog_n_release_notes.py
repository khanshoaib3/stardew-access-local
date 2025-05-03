import argparse
import re
import os
import shutil
import json
import copy_changelogs_to

CHANGELOG_DIR = 'https://github.com/stardew-access/stardew-access/blob/development/docs/changelogs/'
LATEST_FILE_PATH = '../latest.md'
DEFAULT_FILE_PATH = '../default.md'
SEM_VER: str = r"v[0-9]+\.[0-9]+\.[0-9]+.*"
PRE_SEM_VER: str = r"v[0-9]+\.[0-9]+\.[0-9]+-.+"
PRE_SEM_VER_WITH_CAPTURE: str = r"v[0-9]+\.[0-9]+\.[0-9]+-(.+)\.([0-9])"


def main():
    version, release_notes_path, detailed_release_notes, pre_release = get_version_n_output_file_name_from_cli()

    if version == 'auto':
        version = get_version_from_manifest()

    final_changelog_path = f'../{version}.md'

    print(f'Version: {version}')
    print(f'Final changelog file path: {final_changelog_path}')
    print(f'Release notes path: {final_changelog_path}')

    if pre_release == 'auto':
        is_pre_release = ('beta' in version or 'alpha' in version)
    else:
        is_pre_release = True if pre_release == 'true' else False
    print(f'Is pre release: {is_pre_release}')

    gen_final_changelog_file(final_changelog_path, version)
    gen_release_notes(release_notes_path, final_changelog_path,
                      detailed_release_notes, version, is_pre_release)


def gen_final_changelog_file(final_changelog_path: str, version: str):
    print('\nGenerating final changelog...')
    open(final_changelog_path, 'w').close()  # Creates an empty file

    final_changelog = [f'## Changelog {version}', '']

    changelogs_dict = copy_changelogs_to.get_changelogs_dict(LATEST_FILE_PATH)
    for heading, changelogs in changelogs_dict.items():
        if len(changelogs) == 0:
            continue
        print(f'Copying changelogs with heading: {heading}')
        final_changelog += [heading, ''] + changelogs + ['']

    write_list_to_file(final_changelog_path, final_changelog)
    shutil.copyfile(DEFAULT_FILE_PATH, LATEST_FILE_PATH)


def gen_release_notes(release_notes_path: str,
                      changelog_file: str,
                      detailed: bool,
                      version: str,
                      is_pre_release: bool):
    print(f'Detailed release notes generation: {detailed}')
    print('\nGenerating release notes...')
    release_notes = ['## Changelog', '']

    changelogs_dict = copy_changelogs_to.get_changelogs_dict(changelog_file)
    for heading, changelogs in changelogs_dict.items():
        if not detailed and heading != '### New Features' \
           and heading != '### Feature Updates':
            continue
        if heading == '### Translation Changes':
            continue
        if heading == '### Development Chores':
            continue
        if len(changelogs) == 0:
            continue

        print(f'Copying changelogs with heading: {heading}')
        release_notes += [heading, ''] + changelogs + ['']

    print('Adding in links to full changelogs and translation changes...')

    changelog_link = f'{CHANGELOG_DIR}{version}.md'
    release_notes += [f'Full changelog at: {changelog_link}']
    pre_release_before = get_pre_releases_list_before_version(version)
    if not is_pre_release and len(pre_release_before) != 0:
        release_notes += ['', '*Changelogs of pre releases of this version:*']
        for ver in pre_release_before:
            title = get_pre_release_version_title(ver)
            link = f'[link]({CHANGELOG_DIR}{ver}.md)'
            release_notes.append(f'- {title}: {link}')
    release_notes += ['', '*Note: For translators each changelog has a \
`Translation Changes` sub-heading which specifies all the new and modified i18n entries to be translated*']

    write_list_to_file(release_notes_path, release_notes)


def get_pre_release_version_title(version: str) -> str:
    match = re.search(PRE_SEM_VER_WITH_CAPTURE, version)
    if match:
        ver_type = match.group(1)
        ver_type = ver_type if not ver_type == 'rc' else 'Release Candidate'
        return f'{ver_type.title()} {match.group(2)}'
    return ""


def get_pre_releases_list_before_version(version: str) -> list:
    changelog_versions = [f for f in os.listdir('../')
                          if re.match(SEM_VER, f[:-3])]

    if version not in changelog_versions:
        changelog_versions.append(f'{version}.md')
    changelog_versions.sort()

    # Removing extension after sorting because the sorting result
    # is wrong when there is no extension.
    # More specifically, it results in this:
    #     v1.5.11 v1.6.0 v1.6.0-alpha.1 v1.6.0-beta.1
    # When instead it should be like this:
    #     v1.5.11 v1.6.0-alpha.1 v1.6.0-beta.1 v1.6.0
    changelog_versions = [f[:-3] for f in changelog_versions]

    to_return = []
    for i in range(changelog_versions.index(version) - 1, -1, -1):
        if not re.match(PRE_SEM_VER, changelog_versions[i]):
            return to_return
        to_return.append(changelog_versions[i])
    return to_return


def write_list_to_file(file_path: str, lines: list, mode: str = 'w'):
    lines = [f'{line}\n' for line in lines]  # Add trailing line break
    file_object = open(file_path, mode)
    file_object.writelines(lines)
    file_object.close()


def get_version_from_manifest() -> str:
    manifest_json_file = open('../../../stardew-access/manifest.json')
    manifest_json = json.load(manifest_json_file)
    manifest_json_file.close()
    return f"v{manifest_json['Version']}"


def get_version_n_output_file_name_from_cli():
    parser = argparse.ArgumentParser()
    parser.add_argument('-v', '--version', default='auto')
    parser.add_argument('-o', '--output', default='../temp_notes.md')
    parser.add_argument('-d', '--detailed', action='store_true')
    parser.add_argument('-p', '--pre-release', dest='pre_release',
                        default='auto')

    parsed_args = parser.parse_args()
    return [parsed_args.version, parsed_args.output,
            parsed_args.detailed, parsed_args.pre_release]


if __name__ == "__main__":
    main()
