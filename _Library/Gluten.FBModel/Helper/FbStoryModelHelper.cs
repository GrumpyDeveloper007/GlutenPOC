﻿// Ignore Spelling: Fb

using Gluten.FBModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.FBModel.Helper
{
    /// <summary>
    /// Some helper functions to work with the FB story model
    /// </summary>
    public static class FbStoryModelHelper
    {
        /// <summary>
        /// Scan the stories and combine any duplicates (partial)
        /// </summary>
        public static List<Story> CombineStories(List<Story> stories)
        {
            var newStories = new List<Story>();
            foreach (var story in stories)
            {
                if (string.IsNullOrWhiteSpace(story.id)) continue;
                var newStory = newStories.FirstOrDefault(o => o.id == story.id);
                if (newStory == null)
                {
                    newStory = new Story();
                    newStories.Add(newStory);
                }
                newStory.feedback ??= story.feedback;
                newStory.comet_sections ??= story.comet_sections;
                //if (newStory.encrypted_tracking == null) newStory.encrypted_tracking = story.encrypted_tracking;
                newStory.attachments ??= story.attachments;
                newStory.sponsored_data ??= story.sponsored_data;
                newStory.text_format_metadata ??= story.text_format_metadata;
                newStory.post_id ??= story.post_id;
                newStory.actors ??= story.actors;
                newStory.message ??= story.message;
                //if (newStory.ghl_mocked_encrypted_link == null) newStory.ghl_mocked_encrypted_link = story.ghl_mocked_encrypted_link;
                newStory.ghl_label_mocked_cta_button ??= story.ghl_label_mocked_cta_button;
                newStory.wwwURL ??= story.wwwURL;
                newStory.target_group ??= story.target_group;
                newStory.attached_story ??= story.attached_story;
                newStory.id ??= story.id;
                newStory.url ??= story.url;
                newStory.__typename ??= story.__typename;
                newStory.shareable_from_perspective_of_feed_ufi ??= story.shareable_from_perspective_of_feed_ufi;
                newStory.bumpers ??= story.bumpers;
                //if (newStory.tracking == null) newStory.tracking = story.tracking;
                if (newStory.is_text_only_story == false) newStory.is_text_only_story = story.is_text_only_story;
                newStory.message_truncation_line_limit ??= story.message_truncation_line_limit;
                newStory.referenced_sticker ??= story.referenced_sticker;
                newStory.debug_info ??= story.debug_info;
                newStory.serialized_frtp_identifiers ??= story.serialized_frtp_identifiers;
                if (newStory.can_viewer_see_menu == false) newStory.can_viewer_see_menu = story.can_viewer_see_menu;
                newStory.easy_hide_button_story ??= story.easy_hide_button_story;
                if (newStory.creation_time == 0) newStory.creation_time = story.creation_time;
                newStory.ghl_label ??= story.ghl_label;
                newStory.privacy_scope ??= story.privacy_scope;
                newStory.collaborators ??= story.collaborators;
                newStory.title ??= story.title;
                newStory.feedback_context ??= story.feedback_context;
                newStory.story_ufi_container ??= story.story_ufi_container;
                newStory.inform_treatment_for_messaging ??= story.inform_treatment_for_messaging;
                newStory.__module_operation_useCometUFIAdaptivePostActionBar_story ??= story.__module_operation_useCometUFIAdaptivePostActionBar_story;
                newStory.__module_component_useCometUFIAdaptivePostActionBar_story ??= story.__module_component_useCometUFIAdaptivePostActionBar_story;
                newStory.vote_attachments ??= story.vote_attachments;

                // Merge child level items - TODO: For completeness should merge the entire tree, but this will do for now
                if (newStory?.feedback_context != null && newStory?.feedback_context?.interesting_top_level_comments == null) newStory.feedback_context.interesting_top_level_comments = story.feedback_context.interesting_top_level_comments;
                if (newStory?.comet_sections != null && newStory?.comet_sections?.action_link == null) newStory.comet_sections.action_link = story?.comet_sections?.action_link;

            }
            return newStories;
        }


        /// <summary>
        /// Scans the tree structure returned by FB and extracts all stories
        /// </summary>
        public static List<Story> FlattenNodes(Node? node)
        {
            var stories = new List<Story>();
            if (node == null) return stories;
            // Story Node paths -
            // node.attached_story.comet_sections
            // node.comet_sections.Story
            // node.groupfeed
            if (node?.attached_story != null)
                stories.AddRange(FlattenNodes(node.attached_story));
            if (node?.comet_sections != null)
                stories.AddRange(FlattenNodes(node.comet_sections));
            if (node?.feedback?.story != null)
                stories.AddRange(FlattenNodes(node.feedback.story));

            if (node?.group_feed?.edges != null)
            {
                foreach (var item in node.group_feed.edges)
                    stories.AddRange(FlattenNodes(item.node));
            }
            return stories;
        }

        /// <summary>
        /// Scans the tree structure returned by FB and extracts all stories
        /// </summary>
        public static List<Story> FlattenNodes(CometSections node)
        {
            var stories = new List<Story>();
            // comet_sections.attached_story
            // comet_sections.metadata[].story
            // comet_sections.title.story
            // comet_sections.actor_photo.story
            // comet_sections.message.story
            // comet_sections.message_container.story
            // comet_sections.context_layout.story
            // comet_sections.story
            // comet_sections.feedback.story
            // comet_sections.content.story
            // comet_sections.call_to_action.story - only for tracking info
            if (node?.attached_story != null)
                stories.AddRange(FlattenNodes(node.attached_story));
            if (node?.metadata != null)
            {
                foreach (var item in node.metadata)
                    stories.AddRange(FlattenNodes(item.story));
            }
            if (node?.title?.story != null)
                stories.AddRange(FlattenNodes(node.title.story));
            if (node?.actor_photo?.story != null)
                stories.AddRange(FlattenNodes(node.actor_photo.story));
            if (node?.message?.story != null)
                stories.AddRange(FlattenNodes(node.message.story));
            if (node?.message_container?.story != null)
                stories.AddRange(FlattenNodes(node.message_container.story));
            if (node?.context_layout?.story != null)
                stories.AddRange(FlattenNodes(node.context_layout.story));
            if (node?.story != null)
                stories.AddRange(FlattenNodes(node.story));
            if (node?.feedback?.story != null)
                stories.AddRange(FlattenNodes(node.feedback.story));
            if (node?.content?.story != null)
                stories.AddRange(FlattenNodes(node.content.story));

            return stories;
        }
        /// <summary>
        /// Scans the tree structure returned by FB and extracts all stories
        /// </summary>
        public static List<Story> FlattenNodes(Story node)
        {
            var stories = new List<Story>
            {
                node
            };
            // story.comet_sections.attached_story
            // story.comet_sections.metadata[].story
            // story.attached_story
            // story.story_ufi_container.story
            if (node?.comet_sections != null)
                stories.AddRange(FlattenNodes(node.comet_sections));
            if (node?.attached_story != null)
                stories.AddRange(FlattenNodes(node.attached_story));
            if (node?.story_ufi_container?.story != null)
                stories.AddRange(FlattenNodes(node.story_ufi_container.story));
            return stories;
        }
    }
}
