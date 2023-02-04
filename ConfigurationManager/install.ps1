param($installPath, $toolsPath, $package, $project)
$asms = $package.AssemblyReferences | %{$_.Name} 
foreach ($reference in $project