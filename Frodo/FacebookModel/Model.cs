using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8618

namespace Frodo.FacebookModel
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ActionLink
    {
        public string __typename { get; set; }
        public bool has_member_profile { get; set; }
        public Group group { get; set; }
        public Actor actor { get; set; }
        public ModuleOperationCometFeedStoryActorLinkStory __module_operation_CometFeedStoryActorLink_story { get; set; }
        public ModuleComponentCometFeedStoryActorLinkStory __module_component_CometFeedStoryActorLink_story { get; set; }
    }

    public class Actor
    {
        public string __typename { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string __isEntity { get; set; }
        public string url { get; set; }
        public string __isActor { get; set; }
        public object work_foreign_entity_info { get; set; }
        public object work_info { get; set; }
        public StoryBucket story_bucket { get; set; }
        public object live_video_for_comet_live_ring { get; set; }
        public object answer_agent_group_id { get; set; }
        public string profile_url { get; set; }
        public ProfilePicture profile_picture { get; set; }
        public bool is_additional_profile_plus { get; set; }
        public object delegate_page { get; set; }
    }

    public class Actor3
    {
        public string __typename { get; set; }
        public string id { get; set; }
        public string __isEntity { get; set; }
        public string url { get; set; }
    }

    public class ActorPhoto
    {
        public string __typename { get; set; }
        public string __isICometStorySection { get; set; }
        public bool is_prod_eligible { get; set; }
        public Story story { get; set; }
        public bool has_commerce_attachment { get; set; }
        public ModuleOperationCometFeedStoryActorPhotoSectionMatchRendererStory __module_operation_CometFeedStoryActorPhotoSectionMatchRenderer_story { get; set; }
        public ModuleComponentCometFeedStoryActorPhotoSectionMatchRendererStory __module_component_CometFeedStoryActorPhotoSectionMatchRenderer_story { get; set; }
    }

    public class AdaptiveUfiActionRenderer
    {
        public string __typename { get; set; }
        public Feedback feedback { get; set; }
        public bool hideLabelForAMA { get; set; }
        public ModuleOperationUseCometUFIAdaptivePostActionBarFeedback __module_operation_useCometUFIAdaptivePostActionBar_feedback { get; set; }
        public ModuleComponentUseCometUFIAdaptivePostActionBarFeedback __module_component_useCometUFIAdaptivePostActionBar_feedback { get; set; }
        public Story story { get; set; }
        public string external_share_url { get; set; }
        public LsDisclosureImpressionCount ls_disclosure_impression_count { get; set; }
    }

    public class AllSubattachments
    {
        public int count { get; set; }
        public List<Node> nodes { get; set; }
    }

    public class Animation
    {
        public string uri_keyframes2 { get; set; }
    }

    public class AssociatedGroup
    {
        public string context_actor_hovercard { get; set; }
        public string id { get; set; }
    }

    public class Attachment
    {
        public List<string> style_list { get; set; }
        public string deduplication_key { get; set; }
        public Target target { get; set; }
        public string __typename { get; set; }
        //public Styles styles { get; set; }
        public object throwbackStyles { get; set; }
        public CometFooterRenderer comet_footer_renderer { get; set; }
        public object comet_footer_disclaimer_renderer { get; set; }
        public Media media { get; set; }
        public AllSubattachments all_subattachments { get; set; }
        public List<object> action_links { get; set; }
    }

    public class Attachment3
    {
        public string mediaset_token { get; set; }
        public string url { get; set; }
        public AllSubattachments all_subattachments { get; set; }
        public object comet_product_tag_feed_overlay_renderer { get; set; }
        public GhlMockedFooterInfo ghl_mocked_footer_info { get; set; }
    }

    public class Attribute
    {
        public string name { get; set; }
        public string val { get; set; }
    }

    public class Author
    {
        public string __typename { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string __isActor { get; set; }
        public ProfilePictureDepth0 profile_picture_depth_0 { get; set; }
        public ProfilePictureDepth1 profile_picture_depth_1 { get; set; }
        public string gender { get; set; }
        public string __isEntity { get; set; }
        public string url { get; set; }
        public object work_info { get; set; }
        public bool is_verified { get; set; }
        public string short_name { get; set; }
        public string subscribe_status { get; set; }
    }

    public class AuthorUserSignalsRenderer
    {
        public string __typename { get; set; }
        public UserSignalsInfo user_signals_info { get; set; }
        public ModuleOperationCometUFICommentActorLinkBadgesComment __module_operation_CometUFICommentActorLinkBadges_comment { get; set; }
        public ModuleComponentCometUFICommentActorLinkBadgesComment __module_component_CometUFICommentActorLinkBadges_comment { get; set; }
    }

    public class Body
    {
        public string text { get; set; }
        public List<object> ranges { get; set; }
    }

    public class BodyRenderer
    {
        public string __typename { get; set; }
        public List<object> delight_ranges { get; set; }
        public List<object> image_ranges { get; set; }
        public List<object> inline_style_ranges { get; set; }
        public List<object> aggregated_ranges { get; set; }
        public List<object> ranges { get; set; }
        public List<object> color_ranges { get; set; }
        public string text { get; set; }
        public ModuleOperationCometUFICommentTextBodyRendererComment __module_operation_CometUFICommentTextBodyRenderer_comment { get; set; }
        public ModuleComponentCometUFICommentTextBodyRendererComment __module_component_CometUFICommentTextBodyRenderer_comment { get; set; }
    }

    public class CallToAction
    {
        public string __typename { get; set; }
        public string __isICometStorySection { get; set; }
        public bool is_prod_eligible { get; set; }
        public Story story { get; set; }
        public ModuleOperationCometFeedStoryCallToActionSectionStory __module_operation_CometFeedStoryCallToActionSection_story { get; set; }
        public ModuleComponentCometFeedStoryCallToActionSectionStory __module_component_CometFeedStoryCallToActionSection_story { get; set; }
    }

    public class Child
    {
        public List<Attribute> attributes { get; set; }
        public List<Style> styles { get; set; }
        public string text { get; set; }
        public string tag { get; set; }
        public List<object> children { get; set; }
    }

    public class ClientViewConfig
    {
        public bool can_delay_log_impression { get; set; }
        public bool use_banzai_signal_imp { get; set; }
        public bool use_banzai_vital_imp { get; set; }
    }

    public class CometFooterRenderer
    {
        public string __typename { get; set; }
        public Attachment attachment { get; set; }
        public ModuleOperationCometFeedStoryAttachmentFooterMatchRendererAttachment __module_operation_CometFeedStoryAttachmentFooterMatchRenderer_attachment { get; set; }
        public ModuleComponentCometFeedStoryAttachmentFooterMatchRendererAttachment __module_component_CometFeedStoryAttachmentFooterMatchRenderer_attachment { get; set; }
    }

    public class CometSections
    {
        public string __typename { get; set; }
        public Content content { get; set; }
        public Layout layout { get; set; }
        public object copyright_violation_header { get; set; }
        public object header { get; set; }
        public ContextLayout context_layout { get; set; }
        public object aymt_footer { get; set; }
        public object footer { get; set; }
        public Feedback feedback { get; set; }
        public object outer_footer { get; set; }
        public CallToAction call_to_action { get; set; }
        public object post_inform_treatment { get; set; }
        public object above_message { get; set; }
        public object info_icon { get; set; }
        public object attachment_overlay { get; set; }
        public object attached_story { get; set; }
        public Message message { get; set; }
        public object message_suffix { get; set; }
        public MessageContainer message_container { get; set; }
        public object message_sticker { get; set; }
        public object aggregated_stories { get; set; }
        public ActorPhoto actor_photo { get; set; }
        public List<Metadata> metadata { get; set; }
        public Title title { get; set; }
        public ActionLink action_link { get; set; }
        public object badge { get; set; }
        public object follow_button { get; set; }
    }

    public class CometUfiReactionIconRenderer
    {
        public string __typename { get; set; }
        public ModuleOperationCometUFIReactionStrategyFeedback __module_operation_CometUFIReactionStrategy_feedback { get; set; }
        public ModuleComponentCometUFIReactionStrategyFeedback __module_component_CometUFIReactionStrategy_feedback { get; set; }
    }

    public class CometUfiSummaryAndActionsRenderer
    {
        public string __typename { get; set; }
        public Feedback feedback { get; set; }
        public ModuleOperationCometUFISummaryAndActionsFeedback __module_operation_CometUFISummaryAndActions_feedback { get; set; }
        public ModuleComponentCometUFISummaryAndActionsFeedback __module_component_CometUFISummaryAndActions_feedback { get; set; }
    }

    public class Comment
    {
        public string id { get; set; }
        public string legacy_fbid { get; set; }
        public int depth { get; set; }
        public Body body { get; set; }
        public List<object> attachments { get; set; }
        public bool is_markdown_enabled { get; set; }
        public object community_comment_signal_renderer { get; set; }
        public string comment_menu_tooltip { get; set; }
        public bool should_show_comment_menu { get; set; }
        public Author author { get; set; }
        public bool is_author_weak_reference { get; set; }
        public List<CommentActionLink> comment_action_links { get; set; }
        public Feedback feedback { get; set; }
        public PreferredBody preferred_body { get; set; }
        public BodyRenderer body_renderer { get; set; }
        public object comment_parent { get; set; }
        public bool is_declined_by_group_admin_assistant { get; set; }
        public bool is_gaming_video_comment { get; set; }
        public object timestamp_in_video { get; set; }
        public TranslatabilityForViewer translatability_for_viewer { get; set; }
        public bool written_while_video_was_live { get; set; }
        public GroupCommentInfo group_comment_info { get; set; }
        public object bizweb_comment_info { get; set; }
        public bool has_constituent_badge { get; set; }
        public bool can_viewer_see_subsribe_button { get; set; }
        public bool can_see_constituent_badge_upsell { get; set; }
        public string legacy_token { get; set; }
        public ParentFeedback parent_feedback { get; set; }
        public object question_and_answer_type { get; set; }
        public bool is_author_original_poster { get; set; }
        public bool is_viewer_comment_poster { get; set; }
        public bool is_author_bot { get; set; }
        public bool is_author_non_coworker { get; set; }
        public AuthorUserSignalsRenderer author_user_signals_renderer { get; set; }
        public List<object> author_badge_renderers { get; set; }
        public List<object> identity_badges_web { get; set; }
        public bool can_show_multiple_identity_badges { get; set; }
        public List<object> discoverable_identity_badges_web { get; set; }
        public User user { get; set; }
        public ParentPostStory parent_post_story { get; set; }
        public object work_ama_answer_status { get; set; }
        public object work_knowledge_inline_annotation_comment_badge_renderer { get; set; }
        public List<object> business_comment_attributes { get; set; }
        public bool is_live_video_comment { get; set; }
        public int created_time { get; set; }
        public bool translation_available_for_viewer { get; set; }
        public object inline_survey_config { get; set; }
        public string spam_display_mode { get; set; }
        public object attached_story { get; set; }
        public object comment_direct_parent { get; set; }
        public object if_viewer_can_see_member_page_tooltip { get; set; }
        public bool is_disabled { get; set; }
        public object work_answered_event_comment_renderer { get; set; }
        public object comment_upper_badge_renderer { get; set; }
        public object elevated_comment_data { get; set; }
        public object inline_replies_expander_renderer { get; set; }
        public string url { get; set; }
    }

    public class CommentActionLink
    {
        public string __typename { get; set; }
        public Comment comment { get; set; }
        public ModuleOperationCometUFICommentActionLinksComment __module_operation_CometUFICommentActionLinks_comment { get; set; }
        public ModuleComponentCometUFICommentActionLinksComment __module_component_CometUFICommentActionLinks_comment { get; set; }
    }

    public class CommentRenderingInstance
    {
        public Comments comments { get; set; }
    }

    public class Comments
    {
        public int total_count { get; set; }
    }

    public class CommentsCountSummaryRenderer
    {
        public string __typename { get; set; }
        public Feedback feedback { get; set; }
        public ModuleOperationCometUFISummaryBaseFeedback __module_operation_CometUFISummaryBase_feedback { get; set; }
        public ModuleComponentCometUFISummaryBaseFeedback __module_component_CometUFISummaryBase_feedback { get; set; }
    }

    public class CommentsDisabledNoticeRenderer
    {
        public string __typename { get; set; }
        public NoticeMessage notice_message { get; set; }
        public ModuleOperationCometUFICommentDisabledNoticeFeedback __module_operation_CometUFICommentDisabledNotice_feedback { get; set; }
        public ModuleComponentCometUFICommentDisabledNoticeFeedback __module_component_CometUFICommentDisabledNotice_feedback { get; set; }
    }

    public class Consistency
    {
        public int rev { get; set; }
    }

    public class Content
    {
        public string __typename { get; set; }
        public string __isICometStorySection { get; set; }
        public bool is_prod_eligible { get; set; }
        public Story story { get; set; }
        public ModuleOperationCometFeedStoryContentMatchRendererStory __module_operation_CometFeedStoryContentMatchRenderer_story { get; set; }
        public ModuleComponentCometFeedStoryContentMatchRendererStory __module_component_CometFeedStoryContentMatchRenderer_story { get; set; }
    }

    public class ContextLayout
    {
        public string __typename { get; set; }
        public string __isICometStorySection { get; set; }
        public bool is_prod_eligible { get; set; }
        public object local_alerts_story_menu_promotion { get; set; }
        public Story story { get; set; }
        public bool is_regulation_enforced { get; set; }
        public ModuleOperationCometFeedStoryContextSectionMatchRendererStory __module_operation_CometFeedStoryContextSectionMatchRenderer_story { get; set; }
        public ModuleComponentCometFeedStoryContextSectionMatchRendererStory __module_component_CometFeedStoryContextSectionMatchRenderer_story { get; set; }
    }

    public class Csr1i2uSS
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i2vDd
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i2wUi
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i2xAA
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i2yIu
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i2zCb
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3035
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i31Hl
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i32L2
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i33CN
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i34OE
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i35UJ
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i36UO
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i37R3
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i38Zc
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i399z
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3aMf
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3bWp
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3cEE
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3dT
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3eCk
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3f7
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3gRS
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3hWu
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3i9T
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3j4B
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3kJ6
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3lNA
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3mOY
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3nFn
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3oS8
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3pKk
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3qVq
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3r6A
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3sAS
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3tHZ
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3uTz
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3vVR
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3wF6
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3xJW
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3yTp
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i3zQM
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i40Uk
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i41N
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i421
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i43B9
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4415
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i45AN
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i46SU
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i47Ek
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i48Lk
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i49Gz
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4aEh
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4bPS
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4c25
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4dJh
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4eQ5
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4fBQ
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4gIL
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4hND
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4iJt
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4jLZ
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4kXn
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4lAn
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4mZg
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4nUe
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4o14
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4pJ
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4qNm
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4rLJ
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4sFR
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4t1r
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4uWB
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4vFl
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4w00
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4xO3
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4yCJ
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i4z2B
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i50Rd
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i51JX
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i52WM
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i53Ww
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i54Bk
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i55Jv
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i56Vo
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i579Z
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i58Zt
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i59LC
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5aO
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5bUW
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5cOn
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5dS5
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5e7R
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5fEz
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5gHN
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5hDS
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5iM
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5jMn
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5kJ5
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5lOX
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5mXA
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class Csr1i5nXR
    {
        public string type { get; set; }
        public string src { get; set; }
    }

    public class CtaButton
    {
        public List<object> attributes { get; set; }
        public List<Style> styles { get; set; }
        public object text { get; set; }
        public string tag { get; set; }
        public List<Child> children { get; set; }
    }

    public class DarkModeImage
    {
        public string uri { get; set; }
    }

    public class Data
    {
        public Node node { get; set; }
        public string cursor { get; set; }
    }

    public class Ddd
    {
        public Hsrp hsrp { get; set; }
        public Jsmods jsmods { get; set; }
        public List<string> allResources { get; set; }
        public TieredResources tieredResources { get; set; }
    }

    public class Description
    {
        public string text { get; set; }
    }

    public class DisplayedUserSignal
    {
        public string id { get; set; }
        public string signal_type_id { get; set; }
        public LightModeImage lightModeImage { get; set; }
        public DarkModeImage darkModeImage { get; set; }
        public TagRenderInfo tag_render_info { get; set; }
        public Title title { get; set; }
    }

    public class Edge
    {
        public bool visible_in_bling_bar { get; set; }
        public Node node { get; set; }
        public string i18n_reaction_count { get; set; }
        public int reaction_count { get; set; }
    }

    public class Extensions
    {
        public List<PrefetchUrisV2> prefetch_uris_v2 { get; set; }
        public bool is_final { get; set; }
        public SrPayload sr_payload { get; set; }
    }

    public class Feedback
    {
        public AssociatedGroup associated_group { get; set; }
        public string id { get; set; }
        public string __typename { get; set; }
        public string __isICometStorySection { get; set; }
        public bool is_prod_eligible { get; set; }
        public Story story { get; set; }
        public ModuleOperationCometFeedStoryFeedbackSectionStory __module_operation_CometFeedStoryFeedbackSection_story { get; set; }
        public ModuleComponentCometFeedStoryFeedbackSectionStory __module_component_CometFeedStoryFeedbackSection_story { get; set; }
        public string subscription_target_id { get; set; }
        public string i18n_reaction_count { get; set; }
        public ImportantReactors important_reactors { get; set; }
        public ReactionCount reaction_count { get; set; }
        public TopReactions top_reactions { get; set; }
        public ReactionDisplayConfig reaction_display_config { get; set; }
        public ViewerActor viewer_actor { get; set; }
        public object viewer_feedback_reaction_info { get; set; }
        public bool can_show_seen_by { get; set; }
        public object if_viewer_can_see_seen_by_member_list { get; set; }
        public IfViewerCannotSeeSeenByMemberList if_viewer_cannot_see_seen_by_member_list { get; set; }
        public string i18n_share_count { get; set; }
        public ShareCount share_count { get; set; }
        public CommentsCountSummaryRenderer comments_count_summary_renderer { get; set; }
        public object associated_video { get; set; }
        public CommentRenderingInstance comment_rendering_instance { get; set; }
        public object page_private_reply { get; set; }
        public object video_view_count { get; set; }
        public object video_view_count_renderer { get; set; }
        public bool is_similar_cqa_question { get; set; }
        public MessageAction message_action { get; set; }
        public List<UfiActionRenderer> ufi_action_renderers { get; set; }
        public bool should_show_reshare_warning { get; set; }
        public List<AdaptiveUfiActionRenderer> adaptive_ufi_action_renderers { get; set; }
        public List<SupportedReactionInfo> supported_reaction_infos { get; set; }
        public CometUfiReactionIconRenderer comet_ufi_reaction_icon_renderer { get; set; }
        public UnifiedReactors unified_reactors { get; set; }
        public Reactors reactors { get; set; }
        public string comment_composer_placeholder { get; set; }
        public int total_reply_count { get; set; }
        public string url { get; set; }
        public List<Plugin> plugins { get; set; }
        public bool can_viewer_comment { get; set; }
        public bool have_comments_been_disabled { get; set; }
        public string default_comment_ordering_mode { get; set; }
        public bool inline_composer_visible_by_default { get; set; }
    }

    public class FeedbackContext
    {
        public FeedbackTargetWithContext feedback_target_with_context { get; set; }
        public List<InterestingTopLevelComment> interesting_top_level_comments { get; set; }
    }

    public class FeedbackTargetWithContext
    {
        public ViewerActor viewer_actor { get; set; }
        public string id { get; set; }
        public OwningProfile owning_profile { get; set; }
        public bool can_viewer_comment { get; set; }
        public CommentRenderingInstance comment_rendering_instance { get; set; }
        public CometUfiSummaryAndActionsRenderer comet_ufi_summary_and_actions_renderer { get; set; }
        public bool is_community_qa_or_qaish_post { get; set; }
        public ThreadingConfig threading_config { get; set; }
        public string url { get; set; }
        public string __typename { get; set; }
        public List<Plugin> plugins { get; set; }
        public string comment_composer_placeholder { get; set; }
        public bool have_comments_been_disabled { get; set; }
        public string default_comment_ordering_mode { get; set; }
        public bool inline_composer_visible_by_default { get; set; }
        public object work_comment_summaries_from_feedback { get; set; }
        public bool are_live_video_comments_disabled { get; set; }
        public bool is_viewer_muted { get; set; }
        public CommentsDisabledNoticeRenderer comments_disabled_notice_renderer { get; set; }
        public object comment_moderation_filter_restriction_notice { get; set; }
    }

    public class Focus
    {
        public double x { get; set; }
        public double y { get; set; }
    }

    public class FutureOfFeedInfo
    {
        public bool should_reverse_message_and_attachment_position { get; set; }
        public bool should_overlay_header { get; set; }
        public int aspect_ratio_update { get; set; }
        public string web_reshare_variant { get; set; }
    }

    public class GhlLabel
    {
        public List<object> attributes { get; set; }
        public List<Style> styles { get; set; }
        public object text { get; set; }
        public string tag { get; set; }
        public List<Child> children { get; set; }
    }

    public class GhlMockedFooterInfo
    {
        public string headline { get; set; }
        public string footer_body { get; set; }
        public string link { get; set; }
        public string meta { get; set; }
        public CtaButton cta_button { get; set; }
    }

    public class Group
    {
        public string id { get; set; }
        public object answer_agent_id { get; set; }
    }

    public class GroupCommentInfo
    {
        public Group group { get; set; }
        public bool is_author_with_member_profile { get; set; }
        public ModuleOperationCometUFICommentActorLinkCommentGroups __module_operation_CometUFICommentActorLink_comment_groups { get; set; }
        public ModuleComponentCometUFICommentActorLinkCommentGroups __module_component_CometUFICommentActorLink_comment_groups { get; set; }
    }

    public class Hblp
    {
        public Consistency consistency { get; set; }
        public RsrcMap rsrcMap { get; set; }
    }

    public class Hsrp
    {
        public Hblp hblp { get; set; }
    }

    public class IconImage
    {
        public string name { get; set; }
    }

    public class IfViewerCannotSeeSeenByMemberList
    {
        public string i18n_reaction_count { get; set; }
        public ReactionCount reaction_count { get; set; }
        public ReactionDisplayConfig reaction_display_config { get; set; }
        public SeenBy seen_by { get; set; }
        public ModuleOperationCometUFISeenByCountFeedbackIfViewerCannotSeeSeenByMemberList __module_operation_CometUFISeenByCount_feedback__if_viewer_cannot_see_seen_by_member_list { get; set; }
        public ModuleComponentCometUFISeenByCountFeedbackIfViewerCannotSeeSeenByMemberList __module_component_CometUFISeenByCount_feedback__if_viewer_cannot_see_seen_by_member_list { get; set; }
        public string id { get; set; }
    }

    public class Image
    {
        public string uri { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }

    public class ImportantReactors
    {
        public List<object> nodes { get; set; }
    }

    public class InterestingTopLevelComment
    {
        public Comment comment { get; set; }
        public RelevantContextualReplies relevant_contextual_replies { get; set; }
    }

    public class Jsmods
    {
        public List<List<object>> require { get; set; }
    }

    public class Layout
    {
        public string __typename { get; set; }
        public string __isICometStorySection { get; set; }
        public bool is_prod_eligible { get; set; }
        public ModuleOperationCometFeedStoryLayoutMatchRendererStory __module_operation_CometFeedStoryLayoutMatchRenderer_story { get; set; }
        public ModuleComponentCometFeedStoryLayoutMatchRendererStory __module_component_CometFeedStoryLayoutMatchRenderer_story { get; set; }
    }

    public class LightModeImage
    {
        public string uri { get; set; }
    }

    public class LsDisclosureImpressionCount
    {
        public int inline { get; set; }
        public int bottom_sheet { get; set; }
    }

    public class Media
    {
        public string __typename { get; set; }
        public bool is_playable { get; set; }
        public Image image { get; set; }
        public ViewerImage viewer_image { get; set; }
        public string id { get; set; }
        public string __isMedia { get; set; }
        public object photo_cix_screen { get; set; }
        public object copyright_banner_info { get; set; }
        public string accessibility_caption { get; set; }
        public Focus focus { get; set; }
        public Owner owner { get; set; }
        public string __isNode { get; set; }
    }

    public class Message
    {
        public string __typename { get; set; }
        public string __isICometStorySection { get; set; }
        public bool is_prod_eligible { get; set; }
        public Story story { get; set; }
        public ModuleOperationCometFeedStoryMessageMatchRendererStory __module_operation_CometFeedStoryMessageMatchRenderer_story { get; set; }
        public ModuleComponentCometFeedStoryMessageMatchRendererStory __module_component_CometFeedStoryMessageMatchRenderer_story { get; set; }
        public List<object> delight_ranges { get; set; }
        public List<object> image_ranges { get; set; }
        public List<object> inline_style_ranges { get; set; }
        public List<object> aggregated_ranges { get; set; }
        public List<object> ranges { get; set; }
        public List<object> color_ranges { get; set; }
        public string text { get; set; }
    }

    public class MessageAction
    {
        public string __typename { get; set; }
        public ModuleOperationUseCometUFIPostActionBarFeedbackMessageAction __module_operation_useCometUFIPostActionBar_feedback_message_action { get; set; }
        public ModuleComponentUseCometUFIPostActionBarFeedbackMessageAction __module_component_useCometUFIPostActionBar_feedback_message_action { get; set; }
    }

    public class MessageContainer
    {
        public string __typename { get; set; }
        public string __isICometStorySection { get; set; }
        public bool is_prod_eligible { get; set; }
        public Story story { get; set; }
        public ModuleOperationCometFeedStoryMessageContainerMatchRendererStory __module_operation_CometFeedStoryMessageContainerMatchRenderer_story { get; set; }
        public ModuleComponentCometFeedStoryMessageContainerMatchRendererStory __module_component_CometFeedStoryMessageContainerMatchRenderer_story { get; set; }
    }

    public class Metadata
    {
        public string __typename { get; set; }
        public string __isICometStorySection { get; set; }
        public bool is_prod_eligible { get; set; }
        public object override_url { get; set; }
        public object video_override_url { get; set; }
        public Story story { get; set; }
        public ModuleOperationCometFeedStoryMetadataSectionMatchRendererStory __module_operation_CometFeedStoryMetadataSectionMatchRenderer_story { get; set; }
        public ModuleComponentCometFeedStoryMetadataSectionMatchRendererStory __module_component_CometFeedStoryMetadataSectionMatchRenderer_story { get; set; }
    }

    public class ModuleComponentCometFeedStoryActorLinkStory
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedStoryActorPhotoSectionMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedStoryAttachmentFooterMatchRendererAttachment
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedStoryAttachmentMatchRendererAttachment
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedStoryCallToActionSectionStory
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedStoryContentMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedStoryContextSectionMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedStoryFeedbackSectionStory
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedStoryLayoutMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedStoryMessageContainerMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedStoryMessageMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedStoryMetadataSectionMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedStoryTitleSectionMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedUFIContainerStory
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometFeedUnitContainerSectionFeedUnit
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometUFICommentActionLinksComment
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometUFICommentActorLinkBadgesComment
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometUFICommentActorLinkCommentGroups
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometUFICommentDisabledNoticeFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometUFICommentTextBodyRendererComment
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometUFIReactionsCountFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometUFIReactionStrategyFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometUFISeenByCountFeedbackIfViewerCannotSeeSeenByMemberList
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometUFISummaryAndActionsFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentCometUFISummaryBaseFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentUseCometUFIAdaptivePostActionBarFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentUseCometUFIAdaptivePostActionBarStory
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentUseCometUFIComposerPluginsFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentUseCometUFIPostActionBarFeedbackMessageAction
    {
        public string __dr { get; set; }
    }

    public class ModuleComponentUseCometUFIPostActionBarFeedbackUfiActionRenderers
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedStoryActorLinkStory
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedStoryActorPhotoSectionMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedStoryAttachmentFooterMatchRendererAttachment
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedStoryAttachmentMatchRendererAttachment
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedStoryCallToActionSectionStory
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedStoryContentMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedStoryContextSectionMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedStoryFeedbackSectionStory
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedStoryLayoutMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedStoryMessageContainerMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedStoryMessageMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedStoryMetadataSectionMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedStoryTitleSectionMatchRendererStory
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedUFIContainerStory
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometFeedUnitContainerSectionFeedUnit
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometUFICommentActionLinksComment
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometUFICommentActorLinkBadgesComment
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometUFICommentActorLinkCommentGroups
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometUFICommentDisabledNoticeFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometUFICommentTextBodyRendererComment
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometUFIReactionsCountFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometUFIReactionStrategyFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometUFISeenByCountFeedbackIfViewerCannotSeeSeenByMemberList
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometUFISummaryAndActionsFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationCometUFISummaryBaseFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationUseCometUFIAdaptivePostActionBarFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationUseCometUFIAdaptivePostActionBarStory
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationUseCometUFIComposerPluginsFeedback
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationUseCometUFIPostActionBarFeedbackMessageAction
    {
        public string __dr { get; set; }
    }

    public class ModuleOperationUseCometUFIPostActionBarFeedbackUfiActionRenderers
    {
        public string __dr { get; set; }
    }

    public class GroupFeed
    {
        public List<Edge> edges { get; set; }
    }

    public class Node
    {
        public GroupFeed group_feed { get; set; }

        public string __typename { get; set; }
        public string __isFeedUnit { get; set; }
        public string __isCacheable { get; set; }
        public string cache_id { get; set; }
        public object debug_info { get; set; }
        public string id { get; set; }
        public object sponsored_data { get; set; }
        public Feedback feedback { get; set; }
        public object is_story_civic { get; set; }
        public List<object> matched_terms { get; set; }
        public string post_id { get; set; }
        public object cix_screen { get; set; }
        public FutureOfFeedInfo future_of_feed_info { get; set; }
        public object attached_story { get; set; }
        public object bumpers { get; set; }
        // TODO:
        public CometSections comet_sections { get; set; }
        public string encrypted_tracking { get; set; }
        public bool should_host_actor_link_in_watch { get; set; }
        public object whatsapp_ad_context { get; set; }
        public object schema_context { get; set; }
        public string click_tracking_linkshim_cb { get; set; }
        public string encrypted_click_tracking { get; set; }
        public ModuleOperationCometFeedUnitContainerSectionFeedUnit __module_operation_CometFeedUnitContainerSection_feedUnit { get; set; }
        public ModuleComponentCometFeedUnitContainerSectionFeedUnit __module_component_CometFeedUnitContainerSection_feedUnit { get; set; }
        public string __isTrackableFeedUnit { get; set; }
        public Trackingdata trackingdata { get; set; }
        public List<int> viewability_config { get; set; }
        public ClientViewConfig client_view_config { get; set; }
        public string __isNode { get; set; }
        public string localized_name { get; set; }
    }

    public class Node2
    {
        public string deduplication_key { get; set; }
        public Media media { get; set; }
        public string url { get; set; }
    }

    public class NoticeMessage
    {
        public List<object> delight_ranges { get; set; }
        public List<object> image_ranges { get; set; }
        public List<object> inline_style_ranges { get; set; }
        public List<object> aggregated_ranges { get; set; }
        public List<object> ranges { get; set; }
        public List<object> color_ranges { get; set; }
        public string text { get; set; }
    }

    public class Owner
    {
        public string __typename { get; set; }
        public string id { get; set; }
    }

    public class OwningProfile
    {
        public string __typename { get; set; }
        public string name { get; set; }
        public string short_name { get; set; }
        public string id { get; set; }
    }

    public class ParentFeedback
    {
        public string id { get; set; }
        public string share_fbid { get; set; }
        public object political_figure_data { get; set; }
        public OwningProfile owning_profile { get; set; }
    }

    public class ParentPostStory
    {
        public List<Attachment> attachments { get; set; }
        public string id { get; set; }
    }

    public class Plugin
    {
        public string __typename { get; set; }
        public string group_id { get; set; }
        public string post_id { get; set; }
        public ModuleOperationUseCometUFIComposerPluginsFeedback __module_operation_useCometUFIComposerPlugins_feedback { get; set; }
        public ModuleComponentUseCometUFIComposerPluginsFeedback __module_component_useCometUFIComposerPlugins_feedback { get; set; }
        public bool? has_avatar { get; set; }
        public string feedback_id { get; set; }
        public object avatar_style_version { get; set; }
        public int? emoji_size { get; set; }
        public ViewerActor viewer_actor { get; set; }
        public bool? should_condense_video_preview { get; set; }
        public string owning_profile_id { get; set; }
    }

    public class PreferredBody
    {
        public string __typename { get; set; }
        public List<object> delight_ranges { get; set; }
        public List<object> image_ranges { get; set; }
        public List<object> inline_style_ranges { get; set; }
        public List<object> aggregated_ranges { get; set; }
        public List<object> ranges { get; set; }
        public List<object> color_ranges { get; set; }
        public string text { get; set; }
        public string translation_type { get; set; }
    }

    public class PrefetchUrisV2
    {
        public string uri { get; set; }
        public object label { get; set; }
    }

    public class PrivacyInfo
    {
        public string icon_name { get; set; }
        public Description description { get; set; }
    }

    public class PrivacyScope
    {
        public IconImage icon_image { get; set; }
        public string description { get; set; }
    }

    public class ProfilePicture
    {
        public string uri { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int scale { get; set; }
    }

    public class ProfilePictureDepth0
    {
        public string uri { get; set; }
    }

    public class ProfilePictureDepth1
    {
        public string uri { get; set; }
    }

    public class ReactionCount
    {
        public int count { get; set; }
        public bool is_empty { get; set; }
    }

    public class ReactionDisplayConfig
    {
        public string reaction_display_strategy { get; set; }
        public object reaction_string_with_viewer { get; set; }
        public object reaction_string_without_viewer { get; set; }
        public ModuleOperationCometUFIReactionsCountFeedback __module_operation_CometUFIReactionsCount_feedback { get; set; }
        public ModuleComponentCometUFIReactionsCountFeedback __module_component_CometUFIReactionsCount_feedback { get; set; }
    }

    public class Reactors
    {
        public int count { get; set; }
        public bool is_empty { get; set; }
        public string count_reduced { get; set; }
    }

    public class RelevantContextualReplies
    {
        public List<object> nodes { get; set; }
    }

    public class GroupRoot
    {
        public string label { get; set; }
        public List<object> path { get; set; }
        public Data data { get; set; }
        public Extensions extensions { get; set; }
    }

    public class RsrcMap
    {
        [JsonProperty("csr:_1i_2u_sS")]
        public Csr1i2uSS csr_1i_2u_sS { get; set; }

        [JsonProperty("csr:_1i_2v_Dd")]
        public Csr1i2vDd csr_1i_2v_Dd { get; set; }

        [JsonProperty("csr:_1i_2w_Ui")]
        public Csr1i2wUi csr_1i_2w_Ui { get; set; }

        [JsonProperty("csr:_1i_2x_aA")]
        public Csr1i2xAA csr_1i_2x_aA { get; set; }

        [JsonProperty("csr:_1i_2y_Iu")]
        public Csr1i2yIu csr_1i_2y_Iu { get; set; }

        [JsonProperty("csr:_1i_2z_Cb")]
        public Csr1i2zCb csr_1i_2z_Cb { get; set; }

        [JsonProperty("csr:_1i_30_35")]
        public Csr1i3035 csr_1i_30_35 { get; set; }

        [JsonProperty("csr:_1i_31_hl")]
        public Csr1i31Hl csr_1i_31_hl { get; set; }

        [JsonProperty("csr:_1i_32_l2")]
        public Csr1i32L2 csr_1i_32_l2 { get; set; }

        [JsonProperty("csr:_1i_33_CN")]
        public Csr1i33CN csr_1i_33_CN { get; set; }

        [JsonProperty("csr:_1i_34_oE")]
        public Csr1i34OE csr_1i_34_oE { get; set; }

        [JsonProperty("csr:_1i_35_uJ")]
        public Csr1i35UJ csr_1i_35_uJ { get; set; }

        [JsonProperty("csr:_1i_36_UO")]
        public Csr1i36UO csr_1i_36_UO { get; set; }

        [JsonProperty("csr:_1i_37_r3")]
        public Csr1i37R3 csr_1i_37_r3 { get; set; }

        [JsonProperty("csr:_1i_38_Zc")]
        public Csr1i38Zc csr_1i_38_Zc { get; set; }

        [JsonProperty("csr:_1i_39_9z")]
        public Csr1i399z csr_1i_39_9z { get; set; }

        [JsonProperty("csr:_1i_3a_Mf")]
        public Csr1i3aMf csr_1i_3a_Mf { get; set; }

        [JsonProperty("csr:_1i_3b_Wp")]
        public Csr1i3bWp csr_1i_3b_Wp { get; set; }

        [JsonProperty("csr:_1i_3c_EE")]
        public Csr1i3cEE csr_1i_3c_EE { get; set; }

        [JsonProperty("csr:_1i_3d_T/")]
        public Csr1i3dT csr_1i_3d_T { get; set; }

        [JsonProperty("csr:_1i_3e_ck")]
        public Csr1i3eCk csr_1i_3e_ck { get; set; }

        [JsonProperty("csr:_1i_3f_7/")]
        public Csr1i3f7 csr_1i_3f_7 { get; set; }

        [JsonProperty("csr:_1i_3g_rS")]
        public Csr1i3gRS csr_1i_3g_rS { get; set; }

        [JsonProperty("csr:_1i_3h_wu")]
        public Csr1i3hWu csr_1i_3h_wu { get; set; }

        [JsonProperty("csr:_1i_3i_9T")]
        public Csr1i3i9T csr_1i_3i_9T { get; set; }

        [JsonProperty("csr:_1i_3j_4B")]
        public Csr1i3j4B csr_1i_3j_4B { get; set; }

        [JsonProperty("csr:_1i_3k_J6")]
        public Csr1i3kJ6 csr_1i_3k_J6 { get; set; }

        [JsonProperty("csr:_1i_3l_NA")]
        public Csr1i3lNA csr_1i_3l_NA { get; set; }

        [JsonProperty("csr:_1i_3m_oY")]
        public Csr1i3mOY csr_1i_3m_oY { get; set; }

        [JsonProperty("csr:_1i_3n_fn")]
        public Csr1i3nFn csr_1i_3n_fn { get; set; }

        [JsonProperty("csr:_1i_3o_s8")]
        public Csr1i3oS8 csr_1i_3o_s8 { get; set; }

        [JsonProperty("csr:_1i_3p_kk")]
        public Csr1i3pKk csr_1i_3p_kk { get; set; }

        [JsonProperty("csr:_1i_3q_Vq")]
        public Csr1i3qVq csr_1i_3q_Vq { get; set; }

        [JsonProperty("csr:_1i_3r_6A")]
        public Csr1i3r6A csr_1i_3r_6A { get; set; }

        [JsonProperty("csr:_1i_3s_AS")]
        public Csr1i3sAS csr_1i_3s_AS { get; set; }

        [JsonProperty("csr:_1i_3t_HZ")]
        public Csr1i3tHZ csr_1i_3t_HZ { get; set; }

        [JsonProperty("csr:_1i_3u_Tz")]
        public Csr1i3uTz csr_1i_3u_Tz { get; set; }

        [JsonProperty("csr:_1i_3v_vR")]
        public Csr1i3vVR csr_1i_3v_vR { get; set; }

        [JsonProperty("csr:_1i_3w_f6")]
        public Csr1i3wF6 csr_1i_3w_f6 { get; set; }

        [JsonProperty("csr:_1i_3x_JW")]
        public Csr1i3xJW csr_1i_3x_JW { get; set; }

        [JsonProperty("csr:_1i_3y_tp")]
        public Csr1i3yTp csr_1i_3y_tp { get; set; }

        [JsonProperty("csr:_1i_3z_qM")]
        public Csr1i3zQM csr_1i_3z_qM { get; set; }

        [JsonProperty("csr:_1i_40_Uk")]
        public Csr1i40Uk csr_1i_40_Uk { get; set; }

        [JsonProperty("csr:_1i_41_+n")]
        public Csr1i41N csr_1i_41_n { get; set; }

        [JsonProperty("csr:_1i_42_1/")]
        public Csr1i421 csr_1i_42_1 { get; set; }

        [JsonProperty("csr:_1i_43_b9")]
        public Csr1i43B9 csr_1i_43_b9 { get; set; }

        [JsonProperty("csr:_1i_44_15")]
        public Csr1i4415 csr_1i_44_15 { get; set; }

        [JsonProperty("csr:_1i_45_AN")]
        public Csr1i45AN csr_1i_45_AN { get; set; }

        [JsonProperty("csr:_1i_46_SU")]
        public Csr1i46SU csr_1i_46_SU { get; set; }

        [JsonProperty("csr:_1i_47_ek")]
        public Csr1i47Ek csr_1i_47_ek { get; set; }

        [JsonProperty("csr:_1i_48_Lk")]
        public Csr1i48Lk csr_1i_48_Lk { get; set; }

        [JsonProperty("csr:_1i_49_gz")]
        public Csr1i49Gz csr_1i_49_gz { get; set; }

        [JsonProperty("csr:_1i_4a_eh")]
        public Csr1i4aEh csr_1i_4a_eh { get; set; }

        [JsonProperty("csr:_1i_4b_PS")]
        public Csr1i4bPS csr_1i_4b_PS { get; set; }

        [JsonProperty("csr:_1i_4c_25")]
        public Csr1i4c25 csr_1i_4c_25 { get; set; }

        [JsonProperty("csr:_1i_4d_jh")]
        public Csr1i4dJh csr_1i_4d_jh { get; set; }

        [JsonProperty("csr:_1i_4e_Q5")]
        public Csr1i4eQ5 csr_1i_4e_Q5 { get; set; }

        [JsonProperty("csr:_1i_4f_bQ")]
        public Csr1i4fBQ csr_1i_4f_bQ { get; set; }

        [JsonProperty("csr:_1i_4g_IL")]
        public Csr1i4gIL csr_1i_4g_IL { get; set; }

        [JsonProperty("csr:_1i_4h_ND")]
        public Csr1i4hND csr_1i_4h_ND { get; set; }

        [JsonProperty("csr:_1i_4i_Jt")]
        public Csr1i4iJt csr_1i_4i_Jt { get; set; }

        [JsonProperty("csr:_1i_4j_LZ")]
        public Csr1i4jLZ csr_1i_4j_LZ { get; set; }

        [JsonProperty("csr:_1i_4k_Xn")]
        public Csr1i4kXn csr_1i_4k_Xn { get; set; }

        [JsonProperty("csr:_1i_4l_an")]
        public Csr1i4lAn csr_1i_4l_an { get; set; }

        [JsonProperty("csr:_1i_4m_zg")]
        public Csr1i4mZg csr_1i_4m_zg { get; set; }

        [JsonProperty("csr:_1i_4n_Ue")]
        public Csr1i4nUe csr_1i_4n_Ue { get; set; }

        [JsonProperty("csr:_1i_4o_14")]
        public Csr1i4o14 csr_1i_4o_14 { get; set; }

        [JsonProperty("csr:_1i_4p_/j")]
        public Csr1i4pJ csr_1i_4p_j { get; set; }

        [JsonProperty("csr:_1i_4q_nm")]
        public Csr1i4qNm csr_1i_4q_nm { get; set; }

        [JsonProperty("csr:_1i_4r_LJ")]
        public Csr1i4rLJ csr_1i_4r_LJ { get; set; }

        [JsonProperty("csr:_1i_4s_fR")]
        public Csr1i4sFR csr_1i_4s_fR { get; set; }

        [JsonProperty("csr:_1i_4t_1r")]
        public Csr1i4t1r csr_1i_4t_1r { get; set; }

        [JsonProperty("csr:_1i_4u_WB")]
        public Csr1i4uWB csr_1i_4u_WB { get; set; }

        [JsonProperty("csr:_1i_4v_Fl")]
        public Csr1i4vFl csr_1i_4v_Fl { get; set; }

        [JsonProperty("csr:_1i_4w_00")]
        public Csr1i4w00 csr_1i_4w_00 { get; set; }

        [JsonProperty("csr:_1i_4x_O3")]
        public Csr1i4xO3 csr_1i_4x_O3 { get; set; }

        [JsonProperty("csr:_1i_4y_CJ")]
        public Csr1i4yCJ csr_1i_4y_CJ { get; set; }

        [JsonProperty("csr:_1i_4z_2B")]
        public Csr1i4z2B csr_1i_4z_2B { get; set; }

        [JsonProperty("csr:_1i_50_Rd")]
        public Csr1i50Rd csr_1i_50_Rd { get; set; }

        [JsonProperty("csr:_1i_51_JX")]
        public Csr1i51JX csr_1i_51_JX { get; set; }

        [JsonProperty("csr:_1i_52_wM")]
        public Csr1i52WM csr_1i_52_wM { get; set; }

        [JsonProperty("csr:_1i_53_ww")]
        public Csr1i53Ww csr_1i_53_ww { get; set; }

        [JsonProperty("csr:_1i_54_Bk")]
        public Csr1i54Bk csr_1i_54_Bk { get; set; }

        [JsonProperty("csr:_1i_55_jv")]
        public Csr1i55Jv csr_1i_55_jv { get; set; }

        [JsonProperty("csr:_1i_56_vo")]
        public Csr1i56Vo csr_1i_56_vo { get; set; }

        [JsonProperty("csr:_1i_57_9Z")]
        public Csr1i579Z csr_1i_57_9Z { get; set; }

        [JsonProperty("csr:_1i_58_zt")]
        public Csr1i58Zt csr_1i_58_zt { get; set; }

        [JsonProperty("csr:_1i_59_lC")]
        public Csr1i59LC csr_1i_59_lC { get; set; }

        [JsonProperty("csr:_1i_5a_o+")]
        public Csr1i5aO csr_1i_5a_o { get; set; }

        [JsonProperty("csr:_1i_5b_UW")]
        public Csr1i5bUW csr_1i_5b_UW { get; set; }

        [JsonProperty("csr:_1i_5c_On")]
        public Csr1i5cOn csr_1i_5c_On { get; set; }

        [JsonProperty("csr:_1i_5d_S5")]
        public Csr1i5dS5 csr_1i_5d_S5 { get; set; }

        [JsonProperty("csr:_1i_5e_7R")]
        public Csr1i5e7R csr_1i_5e_7R { get; set; }

        [JsonProperty("csr:_1i_5f_Ez")]
        public Csr1i5fEz csr_1i_5f_Ez { get; set; }

        [JsonProperty("csr:_1i_5g_hN")]
        public Csr1i5gHN csr_1i_5g_hN { get; set; }

        [JsonProperty("csr:_1i_5h_DS")]
        public Csr1i5hDS csr_1i_5h_DS { get; set; }

        [JsonProperty("csr:_1i_5i_+m")]
        public Csr1i5iM csr_1i_5i_m { get; set; }

        [JsonProperty("csr:_1i_5j_Mn")]
        public Csr1i5jMn csr_1i_5j_Mn { get; set; }

        [JsonProperty("csr:_1i_5k_J5")]
        public Csr1i5kJ5 csr_1i_5k_J5 { get; set; }

        [JsonProperty("csr:_1i_5l_oX")]
        public Csr1i5lOX csr_1i_5l_oX { get; set; }

        [JsonProperty("csr:_1i_5m_XA")]
        public Csr1i5mXA csr_1i_5m_XA { get; set; }

        [JsonProperty("csr:_1i_5n_XR")]
        public Csr1i5nXR csr_1i_5n_XR { get; set; }
    }

    public class SeenBy
    {
        public object count { get; set; }
        public object i18n_seen_by_count { get; set; }
        public bool seen_by_everyone { get; set; }
    }

    public class ShareCount
    {
        public int count { get; set; }
        public bool is_empty { get; set; }
    }

    public class SrPayload
    {
        public Ddd ddd { get; set; }
    }

    public class Story
    {
        public Feedback feedback { get; set; }
        public CometSections comet_sections { get; set; }
        public string encrypted_tracking { get; set; }
        public List<Attachment> attachments { get; set; }
        public object sponsored_data { get; set; }
        public object text_format_metadata { get; set; }
        public string post_id { get; set; }
        public List<Actor> actors { get; set; }
        public Message message { get; set; }
        public string ghl_mocked_encrypted_link { get; set; }
        public object ghl_label_mocked_cta_button { get; set; }
        public string wwwURL { get; set; }
        public TargetGroup target_group { get; set; }
        public object attached_story { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string __typename { get; set; }
        public object shareable_from_perspective_of_feed_ufi { get; set; }
        public object bumpers { get; set; }
        public string tracking { get; set; }
        public bool is_text_only_story { get; set; }
        public object message_truncation_line_limit { get; set; }
        public object referenced_sticker { get; set; }
        public object debug_info { get; set; }
        public object serialized_frtp_identifiers { get; set; }
        public bool can_viewer_see_menu { get; set; }
        public object easy_hide_button_story { get; set; }
        public int creation_time { get; set; }
        public GhlLabel ghl_label { get; set; }
        public PrivacyScope privacy_scope { get; set; }
        public List<object> collaborators { get; set; }
        public object title { get; set; }
        // TODO:
        public FeedbackContext feedback_context { get; set; }
        public StoryUfiContainer story_ufi_container { get; set; }
        public object inform_treatment_for_messaging { get; set; }
        public ModuleOperationUseCometUFIAdaptivePostActionBarStory __module_operation_useCometUFIAdaptivePostActionBar_story { get; set; }
        public ModuleComponentUseCometUFIAdaptivePostActionBarStory __module_component_useCometUFIAdaptivePostActionBar_story { get; set; }
        public List<object> vote_attachments { get; set; }
    }

    public class StoryBucket
    {
        public List<object> nodes { get; set; }
    }

    public class StoryUfiContainer
    {
        public string __typename { get; set; }
        public Story story { get; set; }
        public ModuleOperationCometFeedUFIContainerStory __module_operation_CometFeedUFIContainer_story { get; set; }
        public ModuleComponentCometFeedUFIContainerStory __module_component_CometFeedUFIContainer_story { get; set; }
    }

    public class Style
    {
        public string name { get; set; }
        public string val { get; set; }
        public string __typename { get; set; }
        public string __isStoryAttachmentStyleRendererUnion { get; set; }
        public bool is_prod_eligible { get; set; }
        public Attachment attachment { get; set; }
        public ModuleOperationCometFeedStoryAttachmentMatchRendererAttachment __module_operation_CometFeedStoryAttachmentMatchRenderer_attachment { get; set; }
        public ModuleComponentCometFeedStoryAttachmentMatchRendererAttachment __module_component_CometFeedStoryAttachmentMatchRenderer_attachment { get; set; }
    }

    public class SupportedReactionInfo
    {
        public Animation animation { get; set; }
        public string id { get; set; }
    }

    public class TagRenderInfo
    {
        public string color_variant { get; set; }
        public string layout_type { get; set; }
    }

    public class Target
    {
        public string __typename { get; set; }
        public string id { get; set; }
    }

    public class TargetGroup
    {
        public string id { get; set; }
        public string visibility { get; set; }
        public PrivacyInfo privacy_info { get; set; }
    }

    public class ThreadingConfig
    {
        public string __typename { get; set; }
    }

    public class TieredResources
    {
        public List<string> r { get; set; }
        public List<object> rdfds { get; set; }
        public List<string> rds { get; set; }
    }

    public class Title
    {
        public string __typename { get; set; }
        public string __isICometStorySection { get; set; }
        public bool is_prod_eligible { get; set; }
        public Story story { get; set; }
        public ModuleOperationCometFeedStoryTitleSectionMatchRendererStory __module_operation_CometFeedStoryTitleSectionMatchRenderer_story { get; set; }
        public ModuleComponentCometFeedStoryTitleSectionMatchRendererStory __module_component_CometFeedStoryTitleSectionMatchRenderer_story { get; set; }
        public string text { get; set; }
    }

    public class TopReactions
    {
        public int count { get; set; }
        public List<Edge> edges { get; set; }
    }

    public class Trackingdata
    {
        public string id { get; set; }
    }

    public class TranslatabilityForViewer
    {
        public string source_dialect { get; set; }
    }

    public class UfiActionRenderer
    {
        public string __typename { get; set; }
        public Feedback feedback { get; set; }
        public bool hideLabelForAMA { get; set; }
        public ModuleOperationUseCometUFIPostActionBarFeedbackUfiActionRenderers __module_operation_useCometUFIPostActionBar_feedback__ufi_action_renderers { get; set; }
        public ModuleComponentUseCometUFIPostActionBarFeedbackUfiActionRenderers __module_component_useCometUFIPostActionBar_feedback__ufi_action_renderers { get; set; }
    }

    public class UnifiedReactors
    {
        public int count { get; set; }
    }

    public class User
    {
        public string name { get; set; }
        public ProfilePicture profile_picture { get; set; }
        public string id { get; set; }
    }

    public class UserSignalsInfo
    {
        public List<DisplayedUserSignal> displayed_user_signals { get; set; }
        public bool has_more { get; set; }
        public string overflow_uri { get; set; }
        public int total_count { get; set; }
        public object show_middot { get; set; }
    }

    public class ViewerActor
    {
        public string __typename { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string __isActor { get; set; }
        public ProfilePictureDepth0 profile_picture_depth_0 { get; set; }
        public ProfilePictureDepth1 profile_picture_depth_1 { get; set; }
        public string gender { get; set; }
    }

    public class ViewerImage
    {
        public int height { get; set; }
        public int width { get; set; }
        public string uri { get; set; }
    }


}
