/*******************************************************************************
 (c) 2005-2014 Copyright, Real-Time Innovations, Inc.  All rights reserved.
 RTI grants Licensee a license to use, modify, compile, and create derivative
 works of the Software.  Licensee has the right to distribute object form only
 for use with RTI products.  The Software is provided "as is", with no warranty
 of any type, including any warranty for fitness for any purpose. RTI is under
 no obligation to maintain or support the Software.  RTI shall not be liable for
 any incidental or consequential damages arising out of the use or inability to
 use the software.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
/* hello_world_publisher.cs

   A publication of data of type hello_world

   This file is derived from code automatically generated by the rtiddsgen 
   command:

   rtiddsgen -language C# -example <arch> hello_world.idl

   Example publication of type hello_world automatically generated by 
   'rtiddsgen'. To test them follow these steps:

   (1) Compile this file and the example subscription.

   (2) Start the subscription with the command
       objs\<arch>\hello_world_subscriber <domain_id> <sample_count>
                
   (3) Start the publication with the command
       objs\<arch>\hello_world_publisher <domain_id> <sample_count>

   (4) [Optional] Specify the list of discovery initial peers and 
       multicast receive addresses via an environment variable or a file 
       (in the current working directory) called NDDS_DISCOVERY_PEERS. 

   You can run any number of publishers and subscribers programs, and can 
   add and remove them dynamically from the domain.


   Example:

       To run the example application on domain <domain_id>:

       bin\<Debug|Release>\hello_world_publisher <domain_id> <sample_count>
       bin\<Debug|Release>\hello_world_subscriber <domain_id> <sample_count>

       
modification history
------------ -------       
*/

public class hello_worldPublisher {

    // This function will take the values from the command line parameters
    private static int READ_INTEGER_PARAM( int i, String parameter,
            String[] args, String syntax ) {
        if (args[i].Equals(parameter)) {
            if (i + 1 >= args.Length) {
                Console.WriteLine(syntax);
                return -1;
            }
            return int.Parse(args[i + 1]);
        }
        return 0;
    }

    public static void Main( string[] args ) {

        int domain_id = 0;
        int sample_count = 0;
        int initial_value = 0;
        int dwh = 0;
        int sleep = 0;

        String syntax = "[options] \n"
                + "-domain_id <domain ID> (default: 0)\n"
                + "-sample_count <number of published samples> "
                + "(default: infinite)\n"
                + "-initial_value <first sample value> (default: 0)\n"
                + "-sleep <sleep time in seconds before finishing> "
                + "(default: 0)\n"
                + "-dwh <1|0> Enable/Disable durable writer history "
                + "(default: 0)\n";

        for (int i = 0; i < args.Length; ++i) {
            if (sleep == 0) {
                sleep = READ_INTEGER_PARAM(i, "-sleep", args, syntax);
            }

            if (domain_id == 0) {
                domain_id = READ_INTEGER_PARAM(i, "-domain_id", args, syntax);
            }

            if (sample_count == 0) {
                sample_count = READ_INTEGER_PARAM(i, "-sample_count", args,
                        syntax);
            }

            if (initial_value == 0) {
                initial_value = READ_INTEGER_PARAM(i, "-initial_value", args,
                        syntax);
            }

            if (dwh == 0) {
                dwh = READ_INTEGER_PARAM(i, "-dwh", args, syntax);
            }
        }
        
        /* Uncomment this to turn on additional logging
        NDDS.ConfigLogger.get_instance().set_verbosity_by_category(
            NDDS.LogCategory.NDDS_CONFIG_LOG_CATEGORY_API, 
            NDDS.LogVerbosity.NDDS_CONFIG_LOG_VERBOSITY_STATUS_ALL);
        */

        // --- Run --- //
        try {
            hello_worldPublisher.publish(
                domain_id, sample_count, initial_value, dwh, sleep);
        } catch (DDS.Exception) {
            Console.WriteLine("error in publisher");
        }
    }

    static void publish( int domain_id, int sample_count, int initial_value,
        int dwh, int sleep ) {

        // --- Create participant --- //

        /* To customize participant QoS, use 
         * the configuration file USER_QOS_PROFILES.xml */
        DDS.DomainParticipant participant =
            DDS.DomainParticipantFactory.get_instance().create_participant(
                domain_id,
                DDS.DomainParticipantFactory.PARTICIPANT_QOS_DEFAULT,
                null /* listener */,
                DDS.StatusMask.STATUS_MASK_NONE);
        if (participant == null) {
            shutdown(participant);
            throw new ApplicationException("create_participant error");
        }

        // --- Create publisher --- //

        /* To customize publisher QoS, use 
         * the configuration file USER_QOS_PROFILES.xml */
        DDS.Publisher publisher = participant.create_publisher(
        DDS.DomainParticipant.PUBLISHER_QOS_DEFAULT,
        null /* listener */,
        DDS.StatusMask.STATUS_MASK_NONE);
        if (publisher == null) {
            shutdown(participant);
            throw new ApplicationException("create_publisher error");
        }

        // --- Create topic --- //

        /* Register type before creating topic */
        System.String type_name = hello_worldTypeSupport.get_type_name();
        try {
            hello_worldTypeSupport.register_type(
                participant, type_name);
        } catch (DDS.Exception e) {
            Console.WriteLine("register_type error {0}", e);
            shutdown(participant);
            throw e;
        }

        /* To customize topic QoS, use 
         * the configuration file USER_QOS_PROFILES.xml */
        DDS.Topic topic = participant.create_topic(
            "Example hello_world",
            type_name,
            DDS.DomainParticipant.TOPIC_QOS_DEFAULT,
            null /* listener */,
            DDS.StatusMask.STATUS_MASK_NONE);
        if (topic == null) {
            shutdown(participant);
            throw new ApplicationException("create_topic error");
        }

        // --- Create writer --- //
        /* If you use Durable Writer History, you need to set several 
         * properties. These properties are set in the USER_QOS_PROFILE.xml
         * file, "durable_writer_history_Profile" profile. See that file for
         * further details.
         */
        DDS.DataWriter writer = null;
        if (dwh == 1) {
            writer = publisher.create_datawriter_with_profile(topic,
                "persistence_example_Library",
                "durable_writer_history_Profile",
                null, DDS.StatusMask.STATUS_MASK_ALL);

        } else {
            writer = publisher.create_datawriter_with_profile(topic,
                "persistence_example_Library",
                "persistence_service_Profile",
                null, DDS.StatusMask.STATUS_MASK_ALL);
        }
        if (writer == null) {
            shutdown(participant);
            throw new ApplicationException("create_datawriter error");
        }
        hello_worldDataWriter hello_world_writer =
            (hello_worldDataWriter)writer;

        // --- Write --- //

        /* Create data sample for writing */
        hello_world instance = hello_worldTypeSupport.create_data();
        if (instance == null) {
            shutdown(participant);
            throw new ApplicationException(
                "hello_worldTypeSupport.create_data error");
        }

        /* For a data type that has a key, if the same instance is going to be
           written multiple times, initialize the key here
           and register the keyed instance prior to writing */
        DDS.InstanceHandle_t instance_handle = DDS.InstanceHandle_t.HANDLE_NIL;
        /*
        instance_handle = hello_world_writer.register_instance(instance);
        */

        /* Main loop */
        const System.Int32 send_period = 1000; // milliseconds
        const System.Int32 one_sec = 1000; // milliseconds
        for (int count = 0;
             (sample_count == 0) || (count < sample_count);
             ++count) {
            Console.WriteLine("Writing hello_world, count {0}", initial_value);

            /* Modify the data to be sent here */
            instance.data = initial_value;
            initial_value++;

            try {
                hello_world_writer.write(instance, ref instance_handle);
            } catch (DDS.Exception e) {
                Console.WriteLine("write error {0}", e);
            }

            System.Threading.Thread.Sleep(send_period);
        }

        while (sleep != 0) {
            System.Threading.Thread.Sleep(one_sec);
            sleep--;
        }

        /*
        try {
            hello_world_writer.unregister_instance(
                instance, ref instance_handle);
        } catch(DDS.Exception e) {
            Console.WriteLine("unregister instance error: {0}", e);
        }
        */

        // --- Shutdown --- //

        /* Delete data sample */
        try {
            hello_worldTypeSupport.delete_data(instance);
        } catch (DDS.Exception e) {
            Console.WriteLine(
                "hello_worldTypeSupport.delete_data error: {0}", e);
        }

        /* Delete all entities */
        shutdown(participant);
    }

    static void shutdown(
        DDS.DomainParticipant participant ) {

        /* Delete all entities */

        if (participant != null) {
            participant.delete_contained_entities();
            DDS.DomainParticipantFactory.get_instance().delete_participant(
                ref participant);
        }

        /* RTI Connext provides finalize_instance() method on
           domain participant factory for people who want to release memory
           used by the participant factory. Uncomment the following block of
           code for clean destruction of the singleton. */
        /*
        try {
            DDS.DomainParticipantFactory.finalize_instance();
        } catch (DDS.Exception e) {
            Console.WriteLine("finalize_instance error: {0}", e);
            throw e;
        }
        */
    }
}

