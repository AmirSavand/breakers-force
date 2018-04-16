﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hitpoint : MonoBehaviour
{
    [Header ("Hitpoint")]
    public float hitpoints;
    public float maxHitpoints = 100;

    public bool isInvulnerable = false;
    public bool isDead = false;

    public Color hitColor = Color.red;

    [Header ("Sound")]
    public GameObject audioHolder;
    public AudioSource hitSound;
    public AudioSource deathSound;

    [Header ("Screen shake and vibrate")]
    public float shakeOnDeathDuration = 0.1f;
    public bool vibrateOnDeath = false;

    [Header ("Destroy")]
    public GameObject destroyObjectOnDeath;
    public bool destroySelfOnDeath = true;

    [Header ("Particle system")]
    public GameObject explosionParticle;

    [Header ("Pieces")]
    public GameObject pieces;
    public float piecesForce = 50;

    [Header ("Death rewards and texts")]
    public Bonus deathBonus;
    public int deathStars;
    public int deathScore;
    public Vector3 deathTextFloatOffset = new Vector3 (0, 0, 0);
    public bool enableDamageTextFloat = false;

    [Header ("Global text")]
    public bool updatesGlobalHitpointText = false;

    private SpriteRenderer spriteRenderer;
    private Color spriteColor;
    private Cam cam;
    private Game game;

    void Start ()
    {
        // Set HP to max HP
        hitpoints = maxHitpoints;

        // Get inits
        cam = Camera.main.GetComponent<Cam> ();
        game = GameObject.FindWithTag ("Game").GetComponent<Game> ();
        spriteRenderer = GetComponentInChildren<SpriteRenderer> ();
        spriteColor = spriteRenderer.color;
    }

    public void damage (float amount)
    {	
        // Has HP (alive)
        if (hitpoints > 0) {
		
            // Change color to red
            spriteRenderer.color = hitColor;

            // Revert to original color
            Invoke ("revertColor", 0.05f);

            // No hitsound and text float for invulnerables
            if (!isInvulnerable) {
                
                // Show hit text float
                if (enableDamageTextFloat) {

                    // Show damage text float
                    game.createTextFloat ("-" + amount, game.textFloatHitpointColor, transform.position);
                }

                // Hit sound
                if (hitSound) {
                    hitSound.Play ();
                }
            }
        }

        // Deal damage (if not invulnerable)
        if (!isInvulnerable) {
            hitpoints = Mathf.Clamp (hitpoints -= amount, 0, maxHitpoints);
        }

        // Text update (UI)
        updateHitpoinText ();

        // No HP left (dead)
        if (hitpoints == 0) {

            // Is about to die
            if (!isDead) {

                // Gives stars on death
                if (deathStars > 0) {
                    game.giveStar (deathStars, transform.position + deathTextFloatOffset);
                }

                // Gives score on death
                if (deathScore > 0) {
                    game.giveScore (deathScore, transform.position + deathTextFloatOffset);
                }

                // Death sound
                if (deathSound) {

                    // Sperate the holder then destroy after audio finished
                    if (audioHolder) {
                        audioHolder.transform.parent = null;
                        Destroy (audioHolder, deathSound.clip.length);
                    }

                    // Play the audio then destroy the holder
                    deathSound.Play ();
                }

                // Death bonus
                if (deathBonus) {

                    // Apply bonus to ship
                    game.player.ship.applyBonus (deathBonus);

                    // Show text of bonus
                    game.createTextFloat (deathBonus.floatText, deathBonus.color, transform.position);
                }
            }

            // If should shake camera on death
            if (shakeOnDeathDuration > 0) {
                cam.shake (shakeOnDeathDuration, vibrateOnDeath);
            }

            // If has death piece
            if (pieces) {

                // Activate
                pieces.SetActive (true);

                // Detach from this object
                pieces.transform.parent = null;

                // For each piece
                foreach (Rigidbody2D piece in pieces.GetComponentsInChildren<Rigidbody2D>()) {

                    // Create explosion like force for that piece
                    piece.AddForce (new Vector2 (Random.Range (-5, 5), Random.Range (-5, 5)) * piecesForce);

                    // Set color
                    piece.GetComponent<SpriteRenderer> ().color = spriteColor;

                    // Detatch piece too
                    piece.transform.parent = null;
                }

                // Destroy the piece holder too
                Destroy (pieces);
            }

            // If destroys an object on death
            if (destroyObjectOnDeath) {
                Destroy (destroyObjectOnDeath);
            }

            // Destroy if should self distruct
            if (destroySelfOnDeath) {

                // If has explosion particle
                if (explosionParticle) {
                    Instantiate (explosionParticle, transform.position, transform.rotation);
                }

                // Destroy self
                Destroy (gameObject);
            }

            // Store life status
            isDead = true;
        }
    }

    /**
     * Damage as much as hitpoint
     */
    public void kill ()
    {
        damage (hitpoints);
    }

    /**
     * Heal and clamp (0, max hp)
     */
    public void heal (float value)
    {
        hitpoints = Mathf.Clamp (hitpoints + value, 1, maxHitpoints);
        updateHitpoinText ();
    }

    public void setMaxHitpoints (float value)
    {
        // Set max hitpoints and current hitpoins to value
        maxHitpoints = value;
        hitpoints = value;
    }

    /**
     * Update the global hitpoint text if should
     */
    public void updateHitpoinText ()
    {
        if (updatesGlobalHitpointText) {
            game.hitpointsText.text = Mathf.FloorToInt (hitpoints / maxHitpoints * 100).ToString ();
        }   
    }

    private void revertColor ()
    {
        // Set to original color
        spriteRenderer.color = spriteColor;
    }
}
