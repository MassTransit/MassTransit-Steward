COPYRIGHT = "Copyright 2013 Chris Patterson"

require File.dirname(__FILE__) + "/build_support/BuildUtils.rb"
require File.dirname(__FILE__) + "/build_support/util.rb"
include FileTest
require 'albacore'
require File.dirname(__FILE__) + "/build_support/versioning.rb"

PRODUCT = 'MassTransit.Steward'
CLR_TOOLS_VERSION = 'v4.0.30319'
OUTPUT_PATH = 'bin/Release'

props = {
  :src => File.expand_path("src"),
  :nuget => File.join(File.expand_path("src"), ".nuget", "nuget.exe"),
  :output => File.expand_path("build_output"),
  :artifacts => File.expand_path("build_artifacts"),
  :lib => File.expand_path("lib"),
  :projects => ["MassTransit.Steward"],
  :keyfile => File.expand_path("MassTransit.snk")
}

desc "Cleans, compiles, il-merges, unit tests, prepares examples, packages zip"
task :all => [:default, :package]

desc "**Default**, compiles and runs tests"
task :default => [:clean, :nuget_restore, :compile, :package]

desc "Update the common version information for the build. You can call this task without building."
assemblyinfo :global_version do |asm|
  # Assembly file config
  asm.product_name = PRODUCT
  asm.description = "MassTransit Steward is a service intermediary for intelligent command and request dispatching based on resource availability."
  asm.version = FORMAL_VERSION
  asm.file_version = FORMAL_VERSION
  asm.custom_attributes :AssemblyInformationalVersion => "#{BUILD_VERSION}",
	:ComVisibleAttribute => false,
	:CLSCompliantAttribute => true
  asm.copyright = COPYRIGHT
  asm.output_file = 'src/SolutionVersion.cs'
  asm.namespaces "System", "System.Reflection", "System.Runtime.InteropServices"
end

desc "Prepares the working directory for a new build"
task :clean do
	FileUtils.rm_rf props[:output]
	waitfor { !exists?(props[:output]) }

	FileUtils.rm_rf props[:artifacts]
	waitfor { !exists?(props[:artifacts]) }

	Dir.mkdir props[:output]
	Dir.mkdir props[:artifacts]
end

desc "Cleans, versions, compiles the application and generates build_output/."
task :compile => [:versioning, :global_version, :build4, :tests4, :copy4]

task :copy4 => [:build4] do
  copyOutputFiles File.join(props[:src], "MassTransit.Steward/bin/Release"), "MassTransit.Steward.{dll,pdb,xml}", File.join(props[:output], 'net-4.0-full')
end

desc "Only compiles the application."
msbuild :build4 do |msb|
	msb.properties :Configuration => "Release",
		:Platform => 'Any CPU'
	msb.use :net4
	msb.targets :Clean, :Build
  msb.properties[:SignAssembly] = 'true'
  msb.properties[:AssemblyOriginatorKeyFile] = props[:keyfile]
	msb.solution = 'src/MassTransit.Steward.sln'
end

def copyOutputFiles(fromDir, filePattern, outDir)
	FileUtils.mkdir_p outDir unless exists?(outDir)
	Dir.glob(File.join(fromDir, filePattern)){|file|
		copy(file, outDir) if File.file?(file)
	}
end

desc "Runs unit tests"
nunit :tests35 => [:build35] do |nunit|
          nunit.command = File.join('src', 'packages','NUnit.Runners.2.6.2', 'tools', 'nunit-console.exe')
          nunit.options = "/framework=#{CLR_TOOLS_VERSION}", '/nothread', '/exclude:Integration', '/nologo', '/labels', "\"/xml=#{File.join(props[:artifacts], 'nunit-test-results-net-3.5.xml')}\""
          nunit.assemblies = FileList[File.join(props[:src], "MassTransit.Steward.Tests/bin/Release", "MassTransit.Steward.Tests.dll")]
end

desc "Runs unit tests"
nunit :tests4 => [:build4] do |nunit|
          nunit.command = File.join('src', 'packages','NUnit.Runners.2.6.2', 'tools', 'nunit-console.exe')
          nunit.options = "/framework=#{CLR_TOOLS_VERSION}", '/nothread', '/exclude:Integration', '/nologo', '/labels', "\"/xml=#{File.join(props[:artifacts], 'nunit-test-results-net-4.0.xml')}\""
          nunit.assemblies = FileList[File.join(props[:src], "MassTransit.Steward.Tests/bin/Release", "MassTransit.Steward.Tests.dll")]
end

task :package => [:nuget, :zip_output]

desc "ZIPs up the build results."
zip :zip_output => [:versioning] do |zip|
	zip.directories_to_zip = [props[:output]]
	zip.output_file = "MassTransit.Steward-#{NUGET_VERSION}.zip"
	zip.output_path = props[:artifacts]
end

desc "Restore NuGet Packages"
task :nuget_restore do
  sh "#{props[:nuget]} install #{File.join(props[:src],".nuget","packages.config")} -Source https://nuget.org/api/v2/ -o #{File.join(props[:src],"packages")}"
end

desc "restores missing packages"
msbuild :nuget_restore do |msb|
  msb.use :net4
  msb.targets :RestorePackages
  msb.solution = 'src/MassTransit.StewardService/MassTransit.StewardService.csproj'
end

desc "restores missing packages"
msbuild :nuget_restore do |msb|
  msb.use :net4
  msb.targets :RestorePackages
  msb.solution = 'src/MassTransit.Steward.Tests/MassTransit.Steward.Tests.csproj'
end

desc "restores missing packages"
msbuild :nuget_restore do |msb|
  msb.use :net4
  msb.targets :RestorePackages
  msb.solution = 'src/MassTransit.Steward.Core/MassTransit.Steward.Core.csproj'
end

desc "restores missing packages"
msbuild :nuget_restore do |msb|
  msb.use :net4
  msb.targets :RestorePackages
  msb.solution = 'src/MassTransit.Steward/MassTransit.Steward.csproj'
end



desc "Builds the nuget package"
task :nuget => [:versioning, :create_nuspec] do
  sh "#{props[:nuget]} pack #{props[:artifacts]}/MassTransit.Steward.nuspec /Symbols /OutputDirectory #{props[:artifacts]}"
end

nuspec :create_nuspec do |nuspec|
  nuspec.id = 'MassTransit.Steward'
  nuspec.version = NUGET_VERSION
  nuspec.authors = 'Chris Patterson'
  nuspec.summary = 'MassTransit Service Intermediary'
  nuspec.description = 'MassTransit Steward is a service intermediary for intelligent command and request dispatching based on resource availability.'
  nuspec.title = 'MassTransit.Steward'
  nuspec.projectUrl = 'http://github.com/MassTransit/MassTransit.Steward'
  nuspec.language = "en-US"
  nuspec.licenseUrl = "http://www.apache.org/licenses/LICENSE-2.0"
  nuspec.requireLicenseAcceptance = "false"
  nuspec.iconUrl = 'https://a248.e.akamai.net/camo.github.com/72cb54409c345c49a1a2cda30498dfc78895f476/687474703a2f2f7777772e70686174626f79672e636f6d2f6d742d6c6f676f2e706e67'
  nuspec.dependency "Magnum", "2.1.0"
  nuspec.dependency "MassTransit", "2.7.2"
  nuspec.output_file = File.join(props[:artifacts], 'MassTransit.Steward.nuspec')
  add_files File.join(props[:output]), 'MassTransit.Steward.{dll,pdb,xml}', nuspec
  nuspec.file(File.join(props[:src], "MassTransit.Steward\\**\\*.cs").gsub("/","\\"), "src")
end

def project_outputs(props)
	props[:projects].map{ |p| "src/#{p}/bin/#{BUILD_CONFIG}/#{p}.dll" }.
		concat( props[:projects].map{ |p| "src/#{p}/bin/#{BUILD_CONFIG}/#{p}.exe" } ).
		find_all{ |path| exists?(path) }
end

def get_commit_hash_and_date
	begin
		commit = `git log -1 --pretty=format:%H`
		git_date = `git log -1 --date=iso --pretty=format:%ad`
		commit_date = DateTime.parse( git_date ).strftime("%Y-%m-%d %H%M%S")
	rescue
		commit = "git unavailable"
	end

	[commit, commit_date]
end

def add_files stage, what_dlls, nuspec
  [['net35', 'net-3.5'], ['net40', 'net-4.0'], ['net40-full', 'net-4.0-full']].each{|fw|
    takeFrom = File.join(stage, fw[1], what_dlls)
    Dir.glob(takeFrom).each do |f|
      nuspec.file(f.gsub("/", "\\"), "lib\\#{fw[0]}")
    end
  }
end

def waitfor(&block)
	checks = 0

	until block.call || checks >10
		sleep 0.5
		checks += 1
	end

	raise 'Waitfor timeout expired. Make sure that you aren\'t running something from the build output folders, or that you have browsed to it through Explorer.' if checks > 10
end
