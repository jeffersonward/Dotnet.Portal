using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Dotnet.Portal.Settings.Models;
using Dotnet.Portal.Settings.Repositories;

namespace Dotnet.Portal.Settings.Factories
{
    public class ApplicationSettingsFactory
    {
        public ApplicationSettings ToModel(ApplicationSettingsDto dto)
        {
            var model = new ApplicationSettings
            {
                DefaultHighlightSettings = GetHighlightSettings(dto.DefaultHighlightSettings),
                Solutions = GetSolutions(dto.Solutions)
            };

            foreach (var solution in model.Solutions)
            {
                if (solution.InheritHighlights)
                {
                    Merge(model.DefaultHighlightSettings, solution.HighlightSettings);
                }

                foreach (var project in solution.Projects)
                {
                    if (project.InheritHighlights)
                    {
                        Merge(solution.HighlightSettings, project.HighlightSettings);
                    }
                }
            }
            return model;
        }

        private static List<SolutionSettings> GetSolutions(IEnumerable<SolutionSettingsDto> dtos)
        {
            return (dtos ?? new SolutionSettingsDto[0]).Select(GetSolution).ToList();
        }

        private static SolutionSettings GetSolution(SolutionSettingsDto dto)
        {
            var model = new SolutionSettings
            {
                Name = dto.Name,
                File = dto.File,
                InheritHighlights = dto.InheritHighlights,
                HighlightSettings = GetHighlightSettings(dto.HighlightSettings),
                Projects = GetProjects(dto.Projects)
            };

            return model;
        }

        private static List<ProjectSettings> GetProjects(IEnumerable<ProjectSettingsDto> dtos)
        {
            return (dtos ?? new ProjectSettingsDto[0]).Select(GetProject).ToList();
        }

        private static ProjectSettings GetProject(ProjectSettingsDto dto)
        {
            var model = new ProjectSettings
            {
                Name = dto.Name,
                File = dto.File,
                Watch = dto.Watch,
                InheritHighlights = dto.InheritHighlights,
                HighlightSettings = GetHighlightSettings(dto.HighlightSettings)
            };

            return model;
        }

        private static List<HighlightSetting> GetHighlightSettings(IEnumerable<HighlightSettingDto> dtos)
        {
            return (dtos ?? new HighlightSettingDto[0]).Select(GetHighlightSetting).ToList();
        }

        private static HighlightSetting GetHighlightSetting(HighlightSettingDto dto)
        {
            var model = new HighlightSetting
            {
                Color = (Color)ColorConverter.ConvertFromString(dto.Color),
                Regex = new Regex(dto.Regex)
            };

            return model;
        }

        private static void Merge(IEnumerable<HighlightSetting> source, List<HighlightSetting> target)
        {
            var missing = source.Where(s => target.All(t => t.Regex.ToString() != s.Regex.ToString()));
            target.AddRange(missing);
        }

        public ApplicationSettingsDto ToDto(ApplicationSettings model)
        {
            var dto = new ApplicationSettingsDto
            {
                DefaultHighlightSettings = model.DefaultHighlightSettings.Select(GetHighlightSettingDto).ToArray(),
                Solutions = model.Solutions.Select(GetSolutionDto).ToArray()
            };

            foreach (var solution in dto.Solutions)
            {
                if (solution.InheritHighlights)
                {
                    solution.HighlightSettings = Omit(dto.DefaultHighlightSettings, solution.HighlightSettings);
                }

                foreach (var project in solution.Projects)
                {
                    if (project.InheritHighlights)
                    {
                        project.HighlightSettings = Omit(solution.HighlightSettings, project.HighlightSettings);
                    }
                }
            }

            return dto;
        }

        private static SolutionSettingsDto GetSolutionDto(SolutionSettings model)
        {
            var dto = new SolutionSettingsDto
            {
                Name = model.Name,
                File = model.File,
                InheritHighlights = model.InheritHighlights,
                HighlightSettings = model.HighlightSettings.Select(GetHighlightSettingDto).ToArray(),
                Projects = model.Projects.Select(GetProjectsDto).ToArray()
            };

            return dto;
        }

        private static ProjectSettingsDto GetProjectsDto(ProjectSettings model)
        {
            var dto = new ProjectSettingsDto
            {
                Name = model.Name,
                File = model.File,
                Watch = model.Watch,
                InheritHighlights = model.InheritHighlights,
                HighlightSettings = model.HighlightSettings.Select(GetHighlightSettingDto).ToArray()
            };

            return dto;
        }

        private static HighlightSettingDto GetHighlightSettingDto(HighlightSetting model)
        {
            var dto = new HighlightSettingDto
            {
                Color = "#" + model.Color.R.ToString("X2") + model.Color.G.ToString("X2") + model.Color.B.ToString("X2"),
                Regex = model.Regex.ToString()
            };

            return dto;
        }

        private static HighlightSettingDto[] Omit(HighlightSettingDto[] existing, HighlightSettingDto[] target)
        {
            var unique = target.Where(t => existing.All(e => e.Regex.ToString() != t.Regex.ToString()));
            return unique.ToArray();
        }
    }
}