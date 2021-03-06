################################################################################
# (c) 2005-2014 Copyright, Real-Time Innovations, Inc.  All rights reserved.
# RTI grants Licensee a license to use, modify, compile, and create derivative
# works of the Software.  Licensee has the right to distribute object form only
# for use with RTI products.  The Software is provided "as is", with no warranty
# of any type, including any warranty for fitness for any purpose. RTI is under
# no obligation to maintain or support the Software.  RTI shall not be liable 
# for any incidental or consequential damages arising out of the use or 
# inability to use the software.
################################################################################

use Cwd;

#$TOP_DIRECTORY is the directory where you have executed the script
$TOP_DIRECTORY = cwd();

# We get the working_directory, NDDS_VERSION and the ARCH by command line
$FOLDER_TO_CHECK = $ARGV[0];
$NDDS_VERSION = $ARGV[1];
$ARCH = $ARGV[2];

$NDDS_HOME = "";

# This variable is the NDDSHOME environment variable
# If NDDSHOME is defined, leave it as is, else it is defined by default
if (defined $ENV{'NDDSHOME'}) {
    $NDDS_HOME = $ENV{'NDDSHOME'};
}
else { 
    $NDDS_HOME = $ENV{'RTI_TOOLSDRIVE'} . "/local/preship/ndds/ndds." . 
                        $NDDS_VERSION;
    print "CAUTION: NDDSHOME is not defined, by default we set to\n\t" . 
          "%RTI_TOOLDRIVE%/local/preship/ndds/ndds.{VERSION}\n";
}
#set NDDSHOME
$ENV{'NDDSHOME'} = unix_path($NDDS_HOME);

#set the scripts folder to the PATH
$ENV{'PATH'} = $ENV{'NDDSHOME'} . "/scripts;" . $ENV{'PATH'};

#set PATH
#C/C++/C# architecture
$ENV{'PATH'} = $ENV{'NDDSHOME'} . "/lib/" . $ARCH . ";" . $ENV{'PATH'};
#Java Architecture
$ENV{'PATH'} = $ENV{'NDDSHOME'} . "/lib/" . $ENV{'OS_ARCH'} . "jdk;" . 
               $ENV{'PATH'};                           

# including Java compiler (Javac) in the path
# If JAVAHOME is not defined we defined it by default
if (!defined $ENV{'JAVAHOME'}) {
    $ENV{'JAVAHOME'} = $ENV{'RTI_TOOLSDRIVE'} . "/local/applications/Java/" . 
        "PLATFORMSDK/win32/jdk1.7.0_04";
    print "CAUTION: JAVAHOME is not defined, by default we set to\n\t" . 
      "%RTI_TOOLDRIVE%/local/applications/Java/PLATFORMSDK/win32/jdk1.7.0_04\n";
}

$ENV{'PATH'} = $ENV{'JAVAHOME'} . "/bin;" . $ENV{'PATH'};
            
# This solution name is the same one for all dynamic data C/C++ examples
$VS_SOLUTION_NAME_C = "Hello-i86Win32VS2010.sln";

# This solution name is the same one for all dynamic data C# examples
$VS_SOLUTION_NAME_CS = "Hello-i86Win32dotnet4.0.sln";

# This function changes the '\' character by '/' like is used in UNIX
#   input parameter:
#       $path: the string to be converted
#   output parameter:
#       $path = $path using UNIX format
sub unix_path {
    my ($path) = @_;
    ($path = shift) =~ tr!\\!/!;
    return $path;
}

# This function runs the makefile generated with the rtiddsgen 
#   input parameter (they are used in the construction of the makefile name):
#       $language: this is the language which is going to be used to compile
#       $path: relative directory where the example is
#   output parameter:
#       none
sub call_compiler {
    my ($language, $path) = @_;
    
    print_example_name ($path);
    
    my ($compile_string) = "";
    
    # we create the compile string using msbuild compiler and the name of the
    # projects we have defined at the first of the file
    if ($language eq "C" or $language eq "C++") {
        $compile_string = "msbuild " . $VS_SOLUTION_NAME_C;
    }
    elsif ($language eq "Java") {
        $compile_string = "javac -classpath .;\"%NDDSHOME%\"\\class\\" . 
                          "nddsjava.jar *.java";
        
        print $compile_string . "\n";
    } elsif ($language eq "C#") {
        $compile_string = "msbuild " . $VS_SOLUTION_NAME_CS;
    }

    # we need to swap the directory where the example is (where the rtiddsgen
    # has been executed) to run the makefile
    chdir $path;
    
    system $compile_string;
    if ( $? != 0 ) {
        # return to the top directory again
        chdir $TOP_DIRECTORY;
        exit(1);
    }
    
    # return to the top directory again
    chdir $TOP_DIRECTORY;
}

#This function prints the name of the example which is going to be compiled
sub print_example_name {
    my ($folder) = @_;
    print "\n*******************************************************" . 
          "****************\n";
    print "***** EXAMPLE: $folder\n";
    print "*********************************************************" . 
          "**************\n";
}
 
#dynamic data access union discriminator example
call_compiler ("C", 
    $FOLDER_TO_CHECK . "/dynamic_data_access_union_discriminator/c");

call_compiler ("C++", $FOLDER_TO_CHECK .
    "./dynamic_data_access_union_discriminator/c++");

call_compiler ("C#", 
    $FOLDER_TO_CHECK . "/dynamic_data_access_union_discriminator/cs");

call_compiler ("Java", 
    $FOLDER_TO_CHECK . "/dynamic_data_access_union_discriminator/Java");
        
#dynamic data nested structs example       
call_compiler ("C", $FOLDER_TO_CHECK . "/dynamic_data_nested_structs/c");

call_compiler ("C++", $FOLDER_TO_CHECK . "/dynamic_data_nested_structs/c++");

call_compiler ("C#", $FOLDER_TO_CHECK . "/dynamic_data_nested_structs/cs");

call_compiler ("Java", $FOLDER_TO_CHECK . "/dynamic_data_nested_structs/java");

#dynamic data sequences example
call_compiler ("C", $FOLDER_TO_CHECK . "/dynamic_data_sequences/c");

call_compiler ("C++", $FOLDER_TO_CHECK . "/dynamic_data_sequences/c++");

call_compiler ("C#", $FOLDER_TO_CHECK . "/dynamic_data_sequences/cs");

call_compiler ("Java", $FOLDER_TO_CHECK . "/dynamic_data_sequences/java");
