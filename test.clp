;;
(batch "UnitTest_Setup_M12.clp")
(open "CLIPS_UnitTest.txt" writeFile "a")
;; Put "Amphin Cutthroat" on the battlefield.
(populatePublicZone 1 ?*ZoneType_Battlefield* (create$ 41415))
(populatePlayerZone 1 ?*ZoneType_Hand* (create$ ))
(populatePublicZone 1 ?*ZoneType_Stack* (create$ 41523))
(setTurn 1 ?*Phase_Main1* ?*Step_None* ?*StepSeq_InStep* 1 1 1)
(bind ?object (nth$ 1 (find-all-instances ((?ai AttributedInstance)) (= ?ai:GRP_ID 41523))))
(bind ?object_ref (nth$ 1 (find-all-instances ((?ref IGameObjectReference)) (eq ?ref:NODE_SPECIFIC_ID ?object))))
(bind ?objectId (send ?object get-ZONE_CONSTANT_ID))
(bind ?target (nth$ 1 (find-all-instances ((?ai AttributedInstance)) (= ?ai:GRP_ID 41415))))
(bind ?target_ref (nth$ 1 (find-all-instances ((?ref IGameObjectReference)) (eq ?ref:NODE_SPECIFIC_ID ?target))))
(bind ?targetId (send ?target get-ZONE_CONSTANT_ID))
(bind ?battlefield (nth$ 1 (find-all-instances ((?gz GameZone)) (= ?gz:TYPE ?*ZoneType_Battlefield*))))
(bind ?battlefield_ref (nth$ 1 (find-all-instances ((?ref IGameObjectReference)) (eq ?ref:NODE_SPECIFIC_ID ?battlefield))))
(createTargetSpec ?objectId 1 (create$ ?targetId))
(assert (ResolveStackTop))
;; should create a DoAttachment mechanic and a Layered Effect
(processEffects)
(processEffects)
(processEffects)
;; should create an Attachment
(processEffects)
(processEffects)
;; now check if Attachment works
(bind ?le (nth$ 1 (find-all-instances ((?le LayeredEffect)) TRUE)))
(assert (LayerActivationInit (LAYER (send ?le get-LAYER))
    (EFFECT_ZONE_CONST_ID (send ?le get-EFFECT_ZONE_CONST_ID))
    (TARGETS (send ?le get-TARGETS))) )
;; process LayerActivationInit
(processEffects)
;; process Retract LayerActivationInit
(processEffects)
;; process LayerActivate
(processEffects)
;; check Attachment, LayeredEffect, and Game Effects
(if (and (= 1 (length$ (find-all-instances ((?le LayeredEffect)) TRUE)))
         (= 1 (length$ (find-all-instances ((?att Attachment)) TRUE)))
         (= 1 (length$ (find-all-instances ((?ab AddAbility)) TRUE))) ) then
    (printout writeFile "Enchanted Creature Has Flying -- OK" crlf)
    else
    (printout writeFile "Enchanted Creature Has Flying -- FAIL" crlf)
)
(close writeFile)
(run)
(exit)

;; Legal actions.
(defrule LegalActions::Ability_1006
    (DetermineAvailableMana (PLAYER_ID ?playerId))
    (object (is-a GameClock) (name [GAME_CLOCK]) (GAME_TREE_NODE ?current))
    (object
        (is-a GameZone)
        (name ?bf-name)
        (ZONE_CONSTANT_ID ?bf-id)
        (TYPE ?bfType&:(eq ?bfType ?*ZoneType_Battlefield*)))
    ?bf-ref <-
        (object (is-a GameZoneReference) (GAME_TREE_NODE ?current) (NODE_SPECIFIC_ID ?bf-name))
    ?go <-
        (object
            (is-a AttributedInstance)
            (name ?object-name)
            (ZONE_ID ?bf-id)
            (CONTROLLER ?playerId)
            (IS_TAPPED 0)
            (ZONE_CONSTANT_ID ?objectId)
            (ABILITIES $?abilities&:(member$ 1006 ?abilities)))
    ?object-ref <-
        (object
            (is-a GameObjectReference)
            (NODE_SPECIFIC_ID ?object-name)
            (GAME_TREE_NODE ?current))
    =>
    (addAvailableMana ?objectId ?playerId (create$ ?*ManaColor_X*)))

(bind ?object (nth$ 1 (find-all-instances ((?ai AttributedInstance)) (= ?ai:GRP_ID 41523))))

;; Defines an Ability object.
(defclass MAIN::Ability (is-a USER)
    (role concrete)
    (pattern-match reactive)

    ;; @property GRP_ID     Catalog id of this ability
    (slot GRP_ID (type INTEGER))

    ;; @property LEGAL_ZONES Zone types from which it is legal to activate this ability
    (multislot LEGAL_ZONES (type INTEGER))

    ;; @property LEGAL_SPEED Speed type (instant, sorcery) of activation
    (slot LEGAL_SPEED (type INTEGER))

    ;; @property ABILITY_CATEGORY    Type of ability (mana, static, activated, triggered)
    (slot ABILITY_CATEGORY (type INTEGER))

    ;; @property CMC
    (slot CMC (type INTEGER)) ; converted mana cost

    ;; @property COST Activation Cost of this ability
    (multislot COST (type INSTANCE) (allowed-classes MAIN::ICost))

    ;; @property OPTIONS Option definitions for this ability
    (multislot OPTIONS (type INSTANCE) (allowed-classes MAIN::Option))

    ;; @property ABILITY_MAPS Mapping from activation indexing to ability indices
    (multislot ABILITY_MAPS (type INSTANCE) (allowed-classes MAIN::CardAbilityMap))

    ;; @property PRODUCIBLE_MANA_COLORS The mana colors this ability can produce
    (multislot PRODUCIBLE_MANA_COLORS (type INTEGER))

    ;; @property MECHANIC_TYPES Mechanics held by this object
    (multislot MECHANIC_TYPES (type INTEGER))
)
