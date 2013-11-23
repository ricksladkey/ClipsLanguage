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
